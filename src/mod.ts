import { CommonUtils } from "./CommonUtils";
import { TraderAssortGenerator } from "./TraderAssortGenerator";
import { ItemHelper } from "./ItemHelper";
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

import type { StaticRouterModService } from "@spt/services/mod/staticRouter/StaticRouterModService";

const modName = "SPTHardcoreRules";

class HardcoreRules implements IPreSptLoadMod, IPostSptLoadMod, IPostDBLoadMod
{
    private commonUtils: CommonUtils
    private traderAssortGenerator: TraderAssortGenerator
    private itemHelper: ItemHelper
	
    private logger: ILogger;
    private configServer: ConfigServer;
    private databaseServer: DatabaseServer;
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
	
    public preSptLoad(container: DependencyContainer): void 
    {
        this.logger = container.resolve<ILogger>("WinstonLogger");
        const staticRouterModService = container.resolve<StaticRouterModService>("StaticRouterModService");
		
        // Get config.json settings for the bepinex plugin
        staticRouterModService.registerStaticRouter(`StaticGetConfig${modName}`,
            [{
                url: "/SPTHardcoreRules/GetConfig",
                action: async () => 
                {
                    return JSON.stringify(modConfig);
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
                    let offers = this.ragfairController.getOffers(sessionId, info);

                    if (modConfig.services.flea_market.only_barter_offers && this.traderAssortGenerator.hasCashOffers(offers))
                    {
                        this.commonUtils.logInfo("Found cash offers in flea market search result. Refreshing offers...");
                        this.traderAssortGenerator.refreshRagfairOffers();
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
        this.profileHelper = container.resolve<ProfileHelper>("ProfileHelper");		
        this.ragfairServer = container.resolve<RagfairServer>("RagfairServer");
        this.ragfairOfferGenerator = container.resolve<RagfairOfferGenerator>("RagfairOfferGenerator");
        this.ragfairOfferService = container.resolve<RagfairOfferService>("RagfairOfferService");
        this.ragfairController = container.resolve<RagfairController>("RagfairController");
        this.localeService = container.resolve<LocaleService>("LocaleService");
        this.httpResponseUtil = container.resolve<HttpResponseUtil>("HttpResponseUtil");
        this.vfs = container.resolve<VFS>("VFS");
		
        this.databaseTables = this.databaseServer.getTables();
        this.ragfairConfig = this.configServer.getConfig<IRagfairConfig>(ConfigTypes.RAGFAIR);
        this.traderConfig = this.configServer.getConfig<ITraderConfig>(ConfigTypes.TRADER);
        this.giftsConfig = this.configServer.getConfig<IGiftsConfig>(ConfigTypes.GIFTS);
		
        this.commonUtils = new CommonUtils(this.logger, this.databaseTables, this.localeService);
        this.traderAssortGenerator = new TraderAssortGenerator(this.commonUtils, this.traderConfig, this.databaseTables, this.ragfairOfferGenerator, this.ragfairServer, this.ragfairOfferService);
        this.itemHelper = new ItemHelper(this.commonUtils, this.databaseTables);
		
        if (!modConfig.enabled)
            return;
        
        if (!this.doesFileIntegrityCheckPass())
        {
            modConfig.enabled = false;
            return;
        }

        this.databaseTables.globals.config.RagFair.minUserLevel = modConfig.services.flea_market.min_level;
        if (!modConfig.services.flea_market.enabled)
            this.disableFleaMarket();
	
        if (modConfig.services.disable_insurance)
            this.disableInsurance();
        if (modConfig.services.disable_post_raid_healing)
            this.disablePostRaidHealing();
		
        if (modConfig.traders.disable_fence)
            this.traderAssortGenerator.disableFence();

        if (modConfig.traders.disable_prapor_starting_gifts)
            this.disablePraporStartingGifts();
		
        this.traderAssortGenerator.updateTraderAssorts();
    }
	
    public postSptLoad(): void
    {
        if (!modConfig.enabled)
        {
            this.commonUtils.logInfo("Mod disabled in config.json.");
            return;
        }
		
        this.traderAssortGenerator.refreshRagfairOffers();
    }
    
    private disableFleaMarket(): void
    {
        this.commonUtils.logInfo("Disabling flea market...");
		
        // It's nice to have the flea interface even if we can't buy/sell items, so completely disabling the flea market isn't ideal
        //this.databaseTables.globals.config.RagFair.enabled = false;
		
        // Don't allow any player offers
        this.ragfairConfig.dynamic.offerItemCount.min = 0;
        this.ragfairConfig.dynamic.offerItemCount.max = 0;
		
        // Don't allow the player to create offers regardless of their flea-market rep
        for (const i in this.databaseTables.globals.config.RagFair.maxActiveOfferCount)
        {
            this.databaseTables.globals.config.RagFair.maxActiveOfferCount[i].count = 0;
        }
    }
	
    private disableInsurance(): void
    {
        this.commonUtils.logInfo("Disabling insurance...");
		
        // Prevent user from insuring items from the context menu
        for (const itemID in this.databaseTables.templates.items)
        {
            this.databaseTables.templates.items[itemID]._props.InsuranceDisabled = true;
        }
    }
	
    private disablePostRaidHealing(): void
    {
        this.commonUtils.logInfo("Disabling post-raid healing...");
		
        for (const trader in this.databaseTables.traders)
        {
            this.databaseTables.traders[trader].base.medic = false;
        }
    }

    private disablePraporStartingGifts(): void
    {
        this.commonUtils.logInfo("Disabling Prapor's starting gifts...");

        this.giftsConfig.gifts.PraporGiftDay1.items = [];
        this.giftsConfig.gifts.PraporGiftDay2.items = [];
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