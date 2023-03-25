Is SPT getting too easy? Too much nice gear collecting dust in your stash? Fix that problem with a hardcore playthrough similar to DeadlySlob's hardcore rules for live Tarkov (https://www.twitlonger.com/show/n_1sr500b). I was inspired by Fin's Hardcore Options (https://hub.sp-tarkov.com/files/file/232-fin-s-hardcore-options/) and made a new version that simply implements the hardcore rulesets by DeadlySlob and other streamers. 

This mod is highly customizable, so you can change the configuration to match the level of difficulty you want. Here are the settings you can change in the **config.json** file in this mod:
* **services.flea_market.enabled**: if this is *false*, you can't list new offers, and there won't be any player offers. You can only use the flea-market interface to browse trader offers.
* **services.flea_market.only_barter_offers**: if you've enabled the flea market, you can set this to *false* to disable all offers using currency.
* **services.flea_market.min_level**: the minimum level you have to be in order to use the flea market. If you set this to *1* but disable the flea market, you can use the flea-market interface to browse trader offers, which is convenient. 
* **services.disable_repairs**: this is a misnomer because it doesn't truly disable trader repairs, but it makes them prohibitively expensive. Repair kits are unaffected. 
* **services.disable_insurance**: All items will be blacklisted from insurance, and the insurance screen will not be displayed when loading into a raid. In case you want to turn this off and on during a playthrough, this will not remove insurance for any items that were previously insured. 
* **services.disable_post_raid_healing**: Disables Therapist's post-raid healing.
* **services.disable_scav_raids**: Disables Scav raids.
* **traders.disable_fence**: Allows you to remove all of Fence's offers. You can still sell items to Fence. 
* **traders.barters_only**: Removes all trader offers using currency unless they're explicitly whitelisted below.
* **traders.whitelist**: If only barters are allowed, use this to whitelist items even if they're not barters. You can whitelist individual items in the **items** array or whitelist an item and all of its child items using the **parents** array. For example, if you have the ID for currency (543be5dd4bdc2deb348b4569) in the **parents** array, all three currencies (RUB, USD, and EUR) will be whitelisted. You can find ID's for items using https://db.sp-tarkov.com/search/ or find them in *[SPT-AKI install directory]\Aki_Data\Server\database\templates\items.json*.
* **secureContainer.more_restrictions**: If this is *true*, you can't put anything in secure containers unless you explicitly whitelist them.
* **secureContainer.restrict_whitelisted_items**: If you really need to remove the temptation to put items in your secure container, this will prohibit you from putting items in containers that are allowed in your secure container unless those items are also allowed directly in your secure container. For example, if docs cases are allowed in your secure container but not currency, currency will also be blacklisted from docs cases if this setting is *true*. This setting is to prevent you from "accidentally" putting a container (like a docs case) in your secure container if it contains items that are normally blacklisted from secure containers. 
* **secureContainer.whitelist.inRaid**: The items in this whitelist are applied only while in-raid. You can specify different whitelists for items that are inspected or uninspected.
* **secureContainer.whitelist.inHideout**: The items in this whitelist are applied only while not in-raid.
* **secureContainer.whitelist.global**: The items in this whitelist are applied all the time. Similar to the trader-offer whitelist, you can whitelist individual items in the **items** array or whitelist an item and all of its child items using the **parents** array.

The following items are whitelisted for trader offers by default:
* All currency (you can exchange RUB for USD, etc. as much as you want)
* All inventory containers (item cases, Scav junkboxes, etc.)
* All special items (markers, signal jammers, etc.)
* All maps

The following items are whitelisted for putting in your secure container by default:
* All keys and keycards (but tey must not be examined when in raid)
* Key tool
* Gingy keychain
* Keycard holder
* S I C C case
* Documents case

There are currently two known issues:
* I can't figure out how to blacklist items from trader repairs, so I made them really expensive instead. I can prohibit all traders from being able to repair, but that causes issues in the repair screen.
* **secureContainer.more_restrictions** applies the extra restrictions even if you're a Scav (assuming you've enabled Scav raids). I should be able to fix this in the next release. 

Good luck!