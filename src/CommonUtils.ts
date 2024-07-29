import modConfig from "../config/config.json";
import modTranslations from "../config/translations.json";

import type { ILogger } from "@spt/models/spt/utils/ILogger";
import type { IDatabaseTables } from "@spt/models/spt/server/IDatabaseTables";
import type { LocaleService } from "@spt/services/LocaleService";

export class CommonUtils
{
    private debugMessagePrefix = "[Hardcore Rules] ";
    private translations: Record<string, string>;
    private locale: string;
	
    constructor (private logger: ILogger, private databaseTables: IDatabaseTables, private localeService: LocaleService)
    {
        // Get all translations for the current locale
        this.locale = this.localeService.getDesiredServerLocale();
        this.translations = this.localeService.getLocaleDb();
    }
	
    public logInfo(message: string, alwaysShow = false): void
    {
        if (modConfig.enabled || alwaysShow)
            this.logger.info(this.debugMessagePrefix + message);
    }

    public logWarning(message: string): void
    {
        this.logger.warning(this.debugMessagePrefix + message);
    }

    public logError(message: string): void
    {
        this.logger.error(this.debugMessagePrefix + message);
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

    public getModTranslation(textKey: string): string
    {
        if (modTranslations[textKey] === undefined)
        {
            this.logError(`Cannot find any translations for ${textKey}`);
            return textKey;
        }

        if (modTranslations[textKey][this.locale] !== undefined)
        {
            return modTranslations[textKey][this.locale];
        }
        
        if (modTranslations[textKey].en !== undefined)
        {
            this.logWarning(`Cannot find ${this.locale} translation for ${textKey}. Using English (en) translation instead.`);
            return modTranslations[textKey].en;
        }
        
        if (this.locale === "en")
        {
            this.logError(`Cannot find English (en) translations for ${textKey}`);
        }
        else
        {
            this.logError(`Cannot find English (en) or ${this.locale} translations for ${textKey}`);
        }

        return textKey;
    }
}