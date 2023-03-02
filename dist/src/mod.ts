import { CommonUtils } from "./CommonUtils";
import { TraderAssortGenerator } from "./TraderAssortGenerator";
import { ItemHelper } from "./ItemHelper";
import modConfig from "../config/config.json";

import { DependencyContainer } from "tsyringe";
import { IPreAkiLoadMod } from "@spt-aki/models/external/IPreAkiLoadMod";
import { IPostDBLoadMod } from "@spt-aki/models/external/IPostDBLoadMod";
import { IPostAkiLoadMod } from "@spt-aki/models/external/IPostAkiLoadMod";

import { ILogger } from "@spt-aki/models/spt/utils/ILogger";
import { DatabaseServer } from "@spt-aki/servers/DatabaseServer";
import { IDatabaseTables } from "@spt-aki/models/spt/server/IDatabaseTables";
import { ConfigServer } from "@spt-aki/servers/ConfigServer";
import { ConfigTypes } from "@spt-aki/models/enums/ConfigTypes";
import { RagfairServer } from "@spt-aki/servers/RagfairServer";
import { IRagfairConfig  } from "@spt-aki/models/spt/config/IRagfairConfig";
import { RagfairOfferGenerator } from "@spt-aki/generators/RagfairOfferGenerator";
import { RagfairOfferService } from "@spt-aki/services/RagfairOfferService";
import { ITraderConfig  } from "@spt-aki/models/spt/config/ITraderConfig";
import { ProfileHelper } from "@spt-aki/helpers/ProfileHelper";
import { LocaleService } from "@spt-aki/services/LocaleService";

import { StaticRouterModService } from "@spt-aki/services/mod/staticRouter/StaticRouterModService";
import { DynamicRouterModService } from "@spt-aki/services/mod/dynamicRouter/DynamicRouterModService";

const modName = "Hardcore Rules";

class HardcoreRules implements IPreAkiLoadMod, IPostAkiLoadMod, IPostDBLoadMod
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
    private traderConfig: ITraderConfig;	
    private databaseTables: IDatabaseTables;
    private profileHelper: ProfileHelper;
    private localeService: LocaleService;
	
    public preAkiLoad(container: DependencyContainer): void 
    {
        if (!modConfig.enabled)
            return;
		
        this.logger = container.resolve<ILogger>("WinstonLogger");
        const staticRouterModService = container.resolve<StaticRouterModService>("StaticRouterModService");
        const dynamicRouterModService = container.resolve<DynamicRouterModService>("DynamicRouterModService");
		
        // Game start
        // Needed to pass sessionId for disabling Scav runs and updating container filters
        staticRouterModService.registerStaticRouter(`StaticAkiGameStart${modName}`,
            [{
                url: "/client/game/start",
                action: (url, info, sessionId, output) => 
                {
                    this.onGameStart(sessionId);
                    return output;
                }
            }], "aki"
        );
		
        // Tried using these to have different item properties in and out-of raid, but it doesn't seem to work
        // Raid start
        /*dynamicRouterModService.registerDynamicRouter(`DynamicAkiRaidStart${modName}`,
            [{
                url: "/client/location/getLocalloot",
                action: (url, info, sessionId, output) => 
                {
                    this.updateSecureContainerRestrictions(sessionId, true);
                    return output;
                }
            }], "aki"
        );
		
        // Raid start
        staticRouterModService.registerStaticRouter(`StaticAkiRaidStart${modName}`,
            [{
                url: "/client/raid/configuration",
                action: (request, sessionId, output) => 
                {
                    this.updateSecureContainerRestrictions(sessionId, true);
                    return output;
                }
            }], "aki"
        );*/
		
        // Raid end
        // Needed for updating container filters and Ragfair offers (until I find a better way of doing it)
        staticRouterModService.registerStaticRouter(`StaticAkiRaidEnd${modName}`,
            [{
                url: "/client/match/offline/end",
                action: (url, info, sessionId, output) => 
                {
                    this.updateSecureContainerRestrictions(sessionId, false);
                    this.traderAssortGenerator.refreshRagfairOffers();
                    return output;
                }
            }], "aki"
        );
    }
	
    public postDBLoad(container: DependencyContainer): void
    {
        if (!modConfig.enabled)
            return;
		
        this.databaseServer = container.resolve<DatabaseServer>("DatabaseServer");		
        this.configServer = container.resolve<ConfigServer>("ConfigServer");		
        this.profileHelper = container.resolve<ProfileHelper>("ProfileHelper");		
        this.ragfairServer = container.resolve<RagfairServer>("RagfairServer");
        this.ragfairOfferGenerator = container.resolve<RagfairOfferGenerator>("RagfairOfferGenerator");
        this.ragfairOfferService = container.resolve<RagfairOfferService>("RagfairOfferService");
        this.localeService = container.resolve<LocaleService>("LocaleService");
		
        this.databaseTables = this.databaseServer.getTables();
        this.ragfairConfig = this.configServer.getConfig<IRagfairConfig>(ConfigTypes.RAGFAIR);
        this.traderConfig = this.configServer.getConfig<ITraderConfig>(ConfigTypes.TRADER);
		
        this.commonUtils = new CommonUtils(this.logger, this.databaseTables, this.localeService);
        this.traderAssortGenerator = new TraderAssortGenerator(this.commonUtils, this.traderConfig, this.databaseTables, this.ragfairOfferGenerator, this.ragfairServer, this.ragfairOfferService);
        this.itemHelper = new ItemHelper(this.commonUtils, this.databaseTables);
		
        this.databaseTables.globals.config.RagFair.minUserLevel = modConfig.services.flea_market.min_level;
        if (!modConfig.services.flea_market.enabled)
            this.disableFleaMarket();
	
        if (modConfig.services.disable_insurance)
            this.disableInsurance();
        if (modConfig.services.disable_repairs)
            this.disableTraderRepairs();
        if (modConfig.services.disable_post_raid_healing)
            this.disablePostRaidHealing();
		
        if (!modConfig.traders.enable_fence)
            this.traderAssortGenerator.disableFence();
		
        this.traderAssortGenerator.updateTraderAssorts();
    }
	
    public postAkiLoad(container: DependencyContainer): void
    {
        if (!modConfig.enabled)
            return;
		
        this.traderAssortGenerator.refreshRagfairOffers();
    }
	
    public onGameStart(sessionId): void
    {
        this.updateScavTimer(sessionId);		
        this.updateSecureContainerRestrictions(sessionId, false);
    }
	
    public updateSecureContainerRestrictions(sessionId, inRaid: boolean): void
    {
        const pmcData = this.profileHelper.getPmcProfile(sessionId);
        if (modConfig.secureContainer.more_restrictions)
            this.itemHelper.updateSecureContainerRestrictions(pmcData, inRaid);
    }
	
    private updateScavTimer(sessionId): void
    {
        const pmcData = this.profileHelper.getPmcProfile(sessionId);
        const scavData = this.profileHelper.getScavProfile(sessionId);
		
        if ((scavData.Info === null) || (scavData.Info === undefined))
        {
            this.commonUtils.logInfo("Scav profile hasn't been created yet.");
            return;
        }
		
        if (modConfig.services.disable_scav_raids)
        {
            this.commonUtils.logInfo(`Increasing scav timer for sessionId=${sessionId}...`);
            this.databaseTables.globals.config.SavagePlayCooldown = 2147483647;
            scavData.Info.SavageLockTime = 2147483647;
        }
        else
        {
            // In case somebody disables scav runs and later wants to enable them, we need to reset their Scav timer unless it's plausible
            const worstCooldownFactor = this.getWorstSavageCooldownModifier();
            if (scavData.Info.SavageLockTime - pmcData.Info.LastTimePlayedAsSavage > this.databaseTables.globals.config.SavagePlayCooldown * worstCooldownFactor * 1.1)
            {
                this.commonUtils.logInfo(`Resetting scav timer for sessionId=${sessionId}...`);
                scavData.Info.SavageLockTime = 0;
            }
        }
    }
	
    // Return the highest Scav cooldown factor from Fence's rep levels
    private getWorstSavageCooldownModifier(): number
    {
        let worstCooldownFactor = 0.01;
        for (const level in this.databaseTables.globals.config.FenceSettings.Levels)
        {
            if (this.databaseTables.globals.config.FenceSettings.Levels[level].SavageCooldownModifier > worstCooldownFactor)
                worstCooldownFactor = this.databaseTables.globals.config.FenceSettings.Levels[level].SavageCooldownModifier;
        }
        return worstCooldownFactor;
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
		
        // This doesn't seem to work... the insurance screen still appears for Labs even with unmodified SPT
        /*for (const map in this.databaseTables.locations)
        {
            // "base" is the first item returned from the collection
            if (map.toLowerCase() === "base")
            {
                continue;
            }
			
            this.databaseTables.locations[map].base.Insurance = false;
        }*/

        // Functionally this works, but the insurance screen still appears and looks bugged
        /*for (const trader in this.databaseTables.traders)
        {
            this.databaseTables.traders[trader].base.insurance.availability = false;
        }*/
		
        // This seems to be the best way of doing it because the insurance screen is shown even when disabled for all traders and maps
        for (const itemID in this.databaseTables.templates.items)
        {
            this.databaseTables.templates.items[itemID]._props.InsuranceDisabled = true;
        }
    }
	
    private disableTraderRepairs(): void
    {
        this.commonUtils.logInfo("Disabling trader repairs...");
		
        for (const trader in this.databaseTables.traders)
        {
            // Functionally this works, but the repair screen can still open and looks bugged
            //this.databaseTables.traders[trader].base.repair.availability = false;
			
            // this isn't exactly what I wanted, but... good enough for now. If I can't totally disable traders repairs, at least make them prohibitively expensive
            this.databaseTables.traders[trader].base.repair.currency_coefficient = 666;
            this.databaseTables.traders[trader].base.repair.quality = 0;
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
}

module.exports = { mod: new HardcoreRules() }