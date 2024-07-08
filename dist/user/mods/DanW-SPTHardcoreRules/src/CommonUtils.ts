import modConfig from "../config/config.json";

import type { ILogger } from "@spt/models/spt/utils/ILogger";
import type { IDatabaseTables } from "@spt/models/spt/server/IDatabaseTables";
import type { LocaleService } from "@spt/services/LocaleService";

export class CommonUtils
{
    private debugMessagePrefix = "[Hardcore Rules] ";
    private translations: Record<string, string>;
	
    constructor (private logger: ILogger, private databaseTables: IDatabaseTables, private localeService: LocaleService)
    {
        // Get all translations for the current locale
        this.translations = this.localeService.getLocaleDb();
    }
	
    public logInfo(message: string): void
    {
        if (modConfig.debug)
            this.logger.info(this.debugMessagePrefix + message);
    }
	
    public getTraderName(traderID: string): string
    {
        const translationKey = `${traderID} Nickname`;
        if (translationKey in this.translations)
            return this.translations[translationKey];
		
        // If a key can't be found in the translations dictionary, fall back to the template data
        const trader = this.databaseTables.traders[traderID];
        return trader.base.nickname;
    }
	
    public getItemName(itemID: string): string
    {
        const translationKey = `${itemID} Name`;
        if (translationKey in this.translations)
            return this.translations[translationKey];
		
        // If a key can't be found in the translations dictionary, fall back to the template data
        const item = this.databaseTables.templates.items[itemID];
        return item._name;
    }
}