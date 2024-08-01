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
import type { TraderAssortService } from "@spt/services/TraderAssortService";
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
const hardcoreProfileEditionName = "Hardcore Playthrough";
const hardcoreProfileDescriptionLocaleKey = "launcher-profile_hardcoreplaythrough";

class HardcoreRules implements IPreSptLoadMod, IPostSptLoadMod, IPostDBLoadMod
{
    private commonUtils: CommonUtils
    private traderAssortGenerator: TraderAssortGenerator
    
    private logger: ILogger;
    private configServer: ConfigServer;
    private databaseServer: DatabaseServer;
    private traderAssortService: TraderAssortService;
    private fenceService: FenceService;
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
        
        // Get config.json settings for the bepinex plugin
        staticRouterModService.registerStaticRouter(`StaticGetConfig${modName}`,
            [{
                url: "/SPTHardcoreRules/GetConfig",
                // biome-ignore lint/suspicious/noExplicitAny: <explanation>
                action: async (url: string, info: any, sessionId: string, output: string) => 
                {
                    const profile = this.profileHelper.getFullProfile(sessionId);
                    this.usingHardcoreProfile = modConfig.use_for_all_profiles || (profile.info.edition === hardcoreProfileEditionName);

                    if (modConfig.enabled)
                    {
                        this.commonUtils.logInfo(`Profile edition for ${profile.info.username} is ${profile.info.edition}. Using hardcore rules = ${this.usingHardcoreProfile}`);
                        this.toggleHardcoreRules();
                    }

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
        this.traderAssortService = container.resolve<TraderAssortService>("TraderAssortService");
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
            this.traderAssortService,
            this.ragfairOfferGenerator,
            this.ragfairServer,
            this.ragfairOfferService,
            this.jsonCloner
        );
		
        if (!modConfig.enabled)
        {
            this.commonUtils.logInfo("Mod disabled in config.json", true);
            return;
        }
        
        if (!this.doesFileIntegrityCheckPass())
        {
            modConfig.enabled = false;
            return;
        }

        this.addHardcoreProfile();

        if (modConfig.debug.enabled)
        {
            this.databaseTables.globals.config.RagFair.minUserLevel = modConfig.debug.flea_market_min_level;
        }
    }
	
    public postSptLoad(): void
    {
        if (!modConfig.enabled)
        {
            return;
        }

        this.showTraderIDs();
    }

    private showTraderIDs(): void
    {
        // No need to show any messages if trader offers won't be modified
        if (!modConfig.traders.barters_only && !modConfig.traders.whitelist_only)
        {
            return;
        }

        for (const trader in this.databaseTables.traders)
        {
            const assort = this.databaseTables.traders[trader].assort;

            // Ignore traders who don't sell anything (i.e. Lightkeeper)
            if ((assort === null) || (assort === undefined) || (assort.items.length === 0))
                continue;

            const whitelisted = modConfig.traders.whitelist_traders.includes(trader);
            this.commonUtils.logInfo(`Found trader: ${this.commonUtils.getTraderName(trader)} (ID=${trader}, Whitelisted=${whitelisted})`);
        }
    }

    private addHardcoreProfile(): void
    {
        this.commonUtils.logInfo(`Adding "${hardcoreProfileEditionName}" profile type...`);

        // Clone the SPT "zero to hero" profile type
        const hardcoreProfileType = this.jsonCloner.clone(this.databaseTables.templates.profiles["SPT Zero to hero"]);
        
        // Remove the knife from both BEAR and USEC profiles, which should be the last item added to the "Zero to hero" profile templates
        hardcoreProfileType.bear.character.Inventory.items.pop();
        hardcoreProfileType.usec.character.Inventory.items.pop();
        
        // Get the translated profile description.
        // NOTE: This approach is needed because server translations are loaded directly from JSON files, not the database tables.
        hardcoreProfileType.descriptionLocaleKey = this.commonUtils.getModTranslation(hardcoreProfileDescriptionLocaleKey);
        
        // Add new profile type
        this.databaseTables.templates.profiles[hardcoreProfileEditionName] = hardcoreProfileType;
    }

    private toggleHardcoreRules(): void
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
            this.regenerateTraderOffers();
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

    private regenerateTraderOffers(): void
    {
        this.traderAssortGenerator.updateTraderAssorts(this.usingHardcoreProfile);
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
            this.ragfairConfig.dynamic.offerItemCount = this.jsonCloner.clone(this.originalRagfairOfferCount);
        }

        if (this.originalMaxActiveOfferCount !== undefined)
        {
            this.databaseTables.globals.config.RagFair.maxActiveOfferCount = this.jsonCloner.clone(this.originalMaxActiveOfferCount);
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
            this.giftsConfig.gifts.PraporGiftDay1.items = this.jsonCloner.clone(this.originalPraporGiftDay1Items);
        }

        if (this.originalPraporGiftDay2Items !== undefined)
        {
            this.giftsConfig.gifts.PraporGiftDay2.items = this.jsonCloner.clone(this.originalPraporGiftDay2Items);
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