import { CommonUtils } from "./CommonUtils";
import { TraderAssortGenerator } from "./TraderAssortGenerator";
import modConfig from "../config/config.json";

import type { DependencyContainer } from "tsyringe";
import type { IPreSptLoadMod } from "@spt/models/external/IPreSptLoadMod";
import type { IPostDBLoadMod } from "@spt/models/external/IPostDBLoadMod";
import type { IPostSptLoadMod } from "@spt/models/external/IPostSptLoadMod";

import type { ILogger } from "@spt/models/spt/utils/ILogger";
import type { DatabaseServer } from "@spt/servers/DatabaseServer";
import type { IDatabaseTables } from "@spt/models/spt/server/IDatabaseTables";
import type { ConfigServer } from "@spt/servers/ConfigServer";
import { ConfigTypes } from "@spt/models/enums/ConfigTypes";
import type { TraderController } from "@spt/controllers/TraderController";
import type { FenceService } from "@spt/services/FenceService";
import type { RagfairServer } from "@spt/servers/RagfairServer";
import type { IRagfairConfig  } from "@spt/models/spt/config/IRagfairConfig";
import type { RagfairOfferGenerator } from "@spt/generators/RagfairOfferGenerator";
import type { RagfairOfferService } from "@spt/services/RagfairOfferService";
import type { RagfairController } from "@spt/controllers/RagfairController";
import type { ITraderConfig  } from "@spt/models/spt/config/ITraderConfig";
import type { IGiftsConfig  } from "@spt/models/spt/config/IGiftsConfig";
import type { ProfileHelper } from "@spt/helpers/ProfileHelper";
import type { LocaleService } from "@spt/services/LocaleService";
import type { HttpResponseUtil } from "@spt/utils/HttpResponseUtil";
import type { VFS } from "@spt/utils/VFS";
import type { JsonCloner } from "@spt/utils/cloners/JsonCloner";

import type { MinMax } from "@spt/models/common/MinMax";
import type { IMaxActiveOfferCount } from "@spt/models/eft/common/IGlobals";
import type { Item } from "@spt/models/eft/common/tables/IItem";

import type { StaticRouterModService } from "@spt/services/mod/staticRouter/StaticRouterModService";

const modName = "SPTHardcoreRules";
const hardcoreProfileTypeName = "Hardcore Playthrough";

class HardcoreRules implements IPreSptLoadMod, IPostSptLoadMod, IPostDBLoadMod
{
    private commonUtils: CommonUtils
    private traderAssortGenerator: TraderAssortGenerator
    
    private logger: ILogger;
    private configServer: ConfigServer;
    private databaseServer: DatabaseServer;
    private fenceService: FenceService;
    private traderController: TraderController;
    private ragfairServer: RagfairServer;
    private ragfairConfig: IRagfairConfig;
    private ragfairOfferGenerator: RagfairOfferGenerator;
    private ragfairOfferService: RagfairOfferService;
    private ragfairController: RagfairController;
    private traderConfig: ITraderConfig;
    private giftsConfig: IGiftsConfig;
    private databaseTables: IDatabaseTables;
    private profileHelper: ProfileHelper;
    private localeService: LocaleService;
    private httpResponseUtil: HttpResponseUtil;
    private vfs: VFS;
    private jsonCloner: JsonCloner;

    private originalRagfairOfferCount: MinMax;
    private originalMaxActiveOfferCount : IMaxActiveOfferCount[];
    private originalPraporGiftDay1Items: Item[];
    private originalPraporGiftDay2Items: Item[];

    private usingHardcoreProfile = false;
    private hardcoreRulesApplied = false;
    
    public preSptLoad(container: DependencyContainer): void 
    {
        this.logger = container.resolve<ILogger>("WinstonLogger");
        this.profileHelper = container.resolve<ProfileHelper>("ProfileHelper");
        const staticRouterModService = container.resolve<StaticRouterModService>("StaticRouterModService");

        // Get Profile Server Mods
        // Needed to determine if a hardcore profile is being used
        staticRouterModService.registerStaticRouter(`StaticAkiProfileGet${modName}`,
            [{
                url: "/launcher/server/serverModsUsedByProfile",
                // biome-ignore lint/suspicious/noExplicitAny: <explanation>
                action: async (url: string, info: any, sessionId: string, output: string) => 
                {
                    const profile = this.profileHelper.getFullProfile(sessionId);
                    this.usingHardcoreProfile = profile.info.edition === hardcoreProfileTypeName;

                    this.commonUtils.logInfo(`Profile edition for ${profile.info.username}: ${profile.info.edition}`);

                    if (modConfig.enabled)
                    {
                        const mustRegenerateTraderOffers = this.usingHardcoreProfile !== this.hardcoreRulesApplied;

                        if (this.usingHardcoreProfile)
                        {
                            this.applyHardcoreRules();
                        }
                        else
                        {
                            this.commonUtils.logWarning("Not using a hardcore profile");
                            this.removeHardcoreRules();
                        }

                        if (mustRegenerateTraderOffers)
                        {
                            this.regenerateTraderOffers(sessionId);
                        }
                    }

                    return output;
                }
            }], "aki"
        );

        // Get config.json settings for the bepinex plugin
        staticRouterModService.registerStaticRouter(`StaticGetConfig${modName}`,
            [{
                url: "/SPTHardcoreRules/GetConfig",
                action: async () => 
                {
                    return JSON.stringify({
                        config: modConfig,
                        usingHardcoreProfile : this.usingHardcoreProfile
                    });
                }
            }], "GetConfig"
        ); 

        if (!modConfig.enabled)
        {
            return;
        }

        // Flea market search
        // Needed to removed banned offers from the flea market
        staticRouterModService.registerStaticRouter(`StaticAkiRagfairFind${modName}`,
            [{
                url: "/client/ragfair/find",
                // biome-ignore lint/suspicious/noExplicitAny: <explanation>
                action: async (url: string, info: any, sessionId: string, output: string) => 
                {
                    const updateOffers = modConfig.enabled && this.usingHardcoreProfile;
                    let offers = this.ragfairController.getOffers(sessionId, info);

                    if (updateOffers && modConfig.services.flea_market.only_barter_offers && this.traderAssortGenerator.hasCashOffers(offers))
                    {
                        this.commonUtils.logInfo("Found cash offers in flea market search result. Refreshing offers...");
                        this.traderAssortGenerator.refreshRagfairOffers(this.usingHardcoreProfile);
                        offers = this.ragfairController.getOffers(sessionId, info);
                    }

                    return this.httpResponseUtil.getBody(offers);
                }
            }], "aki"
        );
    }
	
    public postDBLoad(container: DependencyContainer): void
    {
        this.databaseServer = container.resolve<DatabaseServer>("DatabaseServer");
        this.configServer = container.resolve<ConfigServer>("ConfigServer");
        this.traderController = container.resolve<TraderController>("TraderController");
        this.fenceService = container.resolve<FenceService>("FenceService");
        this.ragfairServer = container.resolve<RagfairServer>("RagfairServer");
        this.ragfairOfferGenerator = container.resolve<RagfairOfferGenerator>("RagfairOfferGenerator");
        this.ragfairOfferService = container.resolve<RagfairOfferService>("RagfairOfferService");
        this.ragfairController = container.resolve<RagfairController>("RagfairController");
        this.localeService = container.resolve<LocaleService>("LocaleService");
        this.httpResponseUtil = container.resolve<HttpResponseUtil>("HttpResponseUtil");
        this.vfs = container.resolve<VFS>("VFS");
        this.jsonCloner = container.resolve<JsonCloner>("JsonCloner");
		
        this.databaseTables = this.databaseServer.getTables();
        this.ragfairConfig = this.configServer.getConfig<IRagfairConfig>(ConfigTypes.RAGFAIR);
        this.traderConfig = this.configServer.getConfig<ITraderConfig>(ConfigTypes.TRADER);
        this.giftsConfig = this.configServer.getConfig<IGiftsConfig>(ConfigTypes.GIFTS);
		
        this.commonUtils = new CommonUtils(this.logger, this.databaseTables, this.localeService);
        this.traderAssortGenerator = new TraderAssortGenerator(
            this.commonUtils,
            this.traderConfig,
            this.databaseTables,
            this.traderController,
            this.ragfairOfferGenerator,
            this.ragfairServer,
            this.ragfairOfferService,
            this.jsonCloner
        );
		
        if (!modConfig.enabled)
            return;
        
        if (!this.doesFileIntegrityCheckPass())
        {
            modConfig.enabled = false;
            return;
        }

        this.addHardcoreProfile();

        this.databaseTables.globals.config.RagFair.minUserLevel = modConfig.services.flea_market.min_level;
    }
	
    public postSptLoad(): void
    {
        
    }

    private addHardcoreProfile(): void
    {
        this.commonUtils.logInfo(`Adding "${hardcoreProfileTypeName}" profile type...`);

        // Clone the SPT "zero to hero" profile type
        const hardcoreProfileType = this.jsonCloner.clone(this.databaseTables.templates.profiles["SPT Zero to hero"]);
        
        // Remove the knife
        hardcoreProfileType.bear.character.Inventory.items.pop();

        // Add new profile type
        hardcoreProfileType.descriptionLocaleKey = "launcher-profile_hardcoreplaythrough";
        this.databaseTables.templates.profiles[hardcoreProfileTypeName] = hardcoreProfileType;

        // Add new profile description for all locales
        for (const locale in this.databaseTables.locales.server)
        {
            this.databaseTables.locales.server[locale][hardcoreProfileType.descriptionLocaleKey] = "Hardcore Rules playthrough";
        }
    }

    private applyHardcoreRules(): void
    {
        if (this.hardcoreRulesApplied)
        {
            return;
        }

        this.commonUtils.logInfo("Applying hardcore rules...");

        if (!modConfig.services.flea_market.enabled)
            this.disableFleaMarket();
        
        if (modConfig.traders.disable_fence)
            this.traderAssortGenerator.disableFence();

        if (modConfig.traders.disable_prapor_starting_gifts)
            this.disablePraporStartingGifts();
		
        this.hardcoreRulesApplied = true;

        this.commonUtils.logInfo("Applying hardcore rules...done.");
    }

    private removeHardcoreRules(): void
    {
        if (!this.hardcoreRulesApplied)
        {
            return;
        }

        this.commonUtils.logInfo("Removing hardcore rules...");

        if (!modConfig.services.flea_market.enabled)
            this.enableFleaMarket();
        
        if (modConfig.traders.disable_fence)
            this.traderAssortGenerator.enableFence();
        
        if (modConfig.traders.disable_prapor_starting_gifts)
            this.enablePraporStartingGifts();

        this.hardcoreRulesApplied = false;
    }

    private regenerateTraderOffers(sessionId: string): void
    {
        this.traderAssortGenerator.updateTraderAssorts(sessionId, this.usingHardcoreProfile);
        this.fenceService.generateFenceAssorts();
        this.traderAssortGenerator.refreshRagfairOffers(this.usingHardcoreProfile);
    }
    
    private disableFleaMarket(): void
    {
        if (this.originalRagfairOfferCount === undefined)
        {
            this.originalRagfairOfferCount = this.jsonCloner.clone(this.ragfairConfig.dynamic.offerItemCount);
        }

        if (this.originalMaxActiveOfferCount === undefined)
        {
            this.originalMaxActiveOfferCount = this.jsonCloner.clone(this.databaseTables.globals.config.RagFair.maxActiveOfferCount);
        }

        this.commonUtils.logInfo("Disabling flea market...");
		
        // Don't allow any player offers
        this.ragfairConfig.dynamic.offerItemCount.min = 0;
        this.ragfairConfig.dynamic.offerItemCount.max = 0;
		
        // Don't allow the player to create offers regardless of their flea-market rep
        for (const i in this.databaseTables.globals.config.RagFair.maxActiveOfferCount)
        {
            this.databaseTables.globals.config.RagFair.maxActiveOfferCount[i].count = 0;
        }
    }

    private enableFleaMarket(): void
    {
        this.commonUtils.logInfo("Enabling flea market...");

        if (this.originalRagfairOfferCount !== undefined)
        {
            this.ragfairConfig.dynamic.offerItemCount = this.originalRagfairOfferCount;
        }

        if (this.originalMaxActiveOfferCount !== undefined)
        {
            this.databaseTables.globals.config.RagFair.maxActiveOfferCount = this.originalMaxActiveOfferCount;
        }
    }

    private disablePraporStartingGifts(): void
    {
        if (this.originalPraporGiftDay1Items === undefined)
        {
            this.originalPraporGiftDay1Items = this.jsonCloner.clone(this.giftsConfig.gifts.PraporGiftDay1.items);
        }

        if (this.originalPraporGiftDay2Items === undefined)
        {
            this.originalPraporGiftDay2Items = this.jsonCloner.clone(this.giftsConfig.gifts.PraporGiftDay2.items);
        }

        this.commonUtils.logInfo("Disabling Prapor's starting gifts...");

        this.giftsConfig.gifts.PraporGiftDay1.items = [];
        this.giftsConfig.gifts.PraporGiftDay2.items = [];
    }

    private enablePraporStartingGifts(): void
    {
        this.commonUtils.logInfo("Enabling Prapor's starting gifts...");

        if (this.originalPraporGiftDay1Items !== undefined)
        {
            this.giftsConfig.gifts.PraporGiftDay1.items = this.originalPraporGiftDay1Items;
        }

        if (this.originalPraporGiftDay2Items !== undefined)
        {
            this.giftsConfig.gifts.PraporGiftDay2.items = this.originalPraporGiftDay2Items;
        }
    }

    private doesFileIntegrityCheckPass(): boolean
    {
        const path = `${__dirname}/..`;

        if (modConfig.secureContainer.only_use_whitelists_in_this_mod && !this.vfs.exists(`${path}/../../../BepInEx/plugins/SPTHardcoreRules.dll`))
        {
            this.commonUtils.logError("Cannot find BepInEx/plugins/SPTHardcoreRules.dll. Without it, this mod will NOT work correctly.");
        
            return false;
        }

        return true;
    }
}

module.exports = { mod: new HardcoreRules() }