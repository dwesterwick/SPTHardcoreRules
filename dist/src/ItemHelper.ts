import { CommonUtils } from "./CommonUtils";
import { IDatabaseTables } from "@spt-aki/models/spt/server/IDatabaseTables";
import modConfig from "../config/config.json";

import { ITemplateItem } from "@spt-aki/models/eft/common/tables/ITemplateItem";
import { IPmcData } from "@spt-aki/models/eft/common/IPmcData";

interface IWhitelist
{
    items: string[],
    parents: string[]
}

export class ItemHelper
{
    private originalItemFilters: Record <string, string[]> = {};
	
    constructor(private commonUtils: CommonUtils, private databaseTables: IDatabaseTables)
    {
		
    }
	
    public updateSecureContainerRestrictions(profile: IPmcData, inRaid: boolean): void
    {
        this.commonUtils.logInfo(`Restricting secure containers (inRaid=${inRaid.toString()})...`);
		
        const whitelist = this.getSecureContainerWhitelist(profile, inRaid);
        this.updateSecureContainerRestrictionsForItemArray(this.databaseTables.templates.items, whitelist);
    }
	
    private updateSecureContainerRestrictionsForItemArray(items: Record<string, ITemplateItem>, whitelist: string[]): void
    {
        for (const itemID in items)
        {
            // update secure containers
            if (items[itemID]._props.isSecured)
            {
                this.commonUtils.logInfo(`Restricting secure containers...${itemID} (${this.commonUtils.getItemName(itemID)})`);
				
                this.updateContainerFilters(items[itemID], whitelist, false);
                continue;
            }
			
            // if another type of container is allowed in a secure container, don't allow anything to be placed in it that isn't also allowed in the secure container
            if (modConfig.secureContainer.restrict_whitelisted_items && whitelist.includes(itemID) && (items[itemID]._props.Grids !== null) && (items[itemID]._props.Grids !== undefined))
            {
                this.commonUtils.logInfo(`Restricting whitelisted containers...${itemID} (${this.commonUtils.getItemName(itemID)})`);
				
                // to allow for whitelists to change in/out of raid, the original container filters need to be stored and recalled before updating them
                if (itemID in this.originalItemFilters)
                {
                    items[itemID]._props.Grids[0]._props.filters[0].Filter = this.originalItemFilters[itemID];
                }
                else
                {
                    const originalFilters = items[itemID]._props.Grids[0]._props.filters[0].Filter;
                    this.originalItemFilters[itemID] = originalFilters;
                }
				
                this.updateContainerFilters(items[itemID], whitelist, true);
            }
        }
    }
	
    private getSecureContainerWhitelist(profile: IPmcData, inRaid: boolean): string[]
    {
        let whitelist = this.generateSecureContainerWhitelist(modConfig.secureContainer.whitelist.global, profile, false);
        if (inRaid)
        {
            whitelist = whitelist.concat(this.generateSecureContainerWhitelist(modConfig.secureContainer.whitelist.inRaid.inspected, profile, false));
            whitelist = whitelist.concat(this.generateSecureContainerWhitelist(modConfig.secureContainer.whitelist.inRaid.uninspected, profile, true));
        }
        else
        {
            whitelist = whitelist.concat(this.generateSecureContainerWhitelist(modConfig.secureContainer.whitelist.inHideout, profile, false));
        }
		
        return whitelist;
    }
	
    private updateContainerFilters(containerItem: ITemplateItem, whitelist: string[], innerJoin: boolean)
    {
        const grids = containerItem._props.Grids;
        for (const grid in grids)
        {
            const filters = grids[grid]._props.filters;			
            for (const filter in filters)
            {
                let whitelistArray = whitelist;
                if (innerJoin)
                {
                    whitelistArray = this.innerJoinWithParentSearch(filters[filter].Filter, whitelist);
                }
				
                filters[filter].Filter = whitelistArray;
                filters[filter].ExcludedFilter = [];
            }
        }
    }
	
    /**
     * Returns only the items in array2 that are either in @param arrayWithParents or are children of items in @param arrayWithParents
     */
    public innerJoinWithParentSearch(arrayWithParents: string[], array2: string[]): string[]
    {
        const joinedArray = [];
        for (const i in arrayWithParents)
        {
            if (array2.includes(arrayWithParents[i]))
            {
                if (!joinedArray.includes(arrayWithParents[i]))
                    joinedArray.push(arrayWithParents[i]);
				
                continue;
            }
			
            for (const j in array2)
            {
                if (ItemHelper.hasParent(this.databaseTables.templates.items[array2[j]], arrayWithParents[i], this.databaseTables))
                {
                    if (!joinedArray.includes(array2[j]))
                        joinedArray.push(array2[j]);
                }
            }
        }
        return joinedArray;
    }
	
    private generateSecureContainerWhitelist(whitelistObj: IWhitelist, profile: IPmcData, onlyUninspected: boolean): string[]
    {
        const whitelistArray = [];
        for (const itemID in this.databaseTables.templates.items)
        {
            // Don't whitelist anything that isn't tangible
            if (this.databaseTables.templates.items[itemID]._type.toLowerCase() == "node")
                continue;
			
            // Optionally, don't whitelist items that have been inspected
            if (onlyUninspected && (profile.Encyclopedia !== null) && (profile.Encyclopedia !== undefined) && (itemID in profile.Encyclopedia))
                continue;
			
            if (whitelistObj.items.includes(itemID))
                whitelistArray.push(itemID);
			
            if (ItemHelper.hasAnyParents(this.databaseTables.templates.items[itemID], whitelistObj.parents, this.databaseTables))
                whitelistArray.push(itemID);
        }
        return whitelistArray;
    }
	
    /**
     * Check if @param item is a child of any of the items with ID's @param parentIDs
     */
    public static hasAnyParents(item: ITemplateItem, parentIDs: string[], databaseTables: IDatabaseTables): boolean
    {
        for (const i in parentIDs)
        {
            if (ItemHelper.hasParent(item, parentIDs[i], databaseTables))
                return true;
        }
		
        return false;
    }
	
    /**
     * Check if @param item is a child of the item with ID @param parentID
     */
    public static hasParent(item: ITemplateItem, parentID: string, databaseTables: IDatabaseTables): boolean
    {
        const allParents = ItemHelper.getAllParents(item, databaseTables);
        return allParents.includes(parentID);
    }
	
    public static getAllParents(item: ITemplateItem, databaseTables: IDatabaseTables): string[]
    {
        if ((item._parent === null) || (item._parent === undefined) || (item._parent == ""))
            return [];
		
        const allParents = ItemHelper.getAllParents(databaseTables.templates.items[item._parent], databaseTables);
        allParents.push(item._parent);
		
        return allParents;
    }
}