using HardcoreRules.Utils.Internal;
using HardcoreRules.Utils.OfferSourceUtils.OfferSources.Internal;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Services;

namespace HardcoreRules.Utils.OfferSourceUtils.OfferSources
{
    internal class TraderOfferSource : AbstractOfferSource
    {
        private LoggingUtil _loggingUtil;
        private DatabaseService _databaseService;

        private Action _restrictTraderOffersAction;

        private Dictionary<MongoId, ObjectCache<TraderAssort>> _originalTraderAssorts = new();
        private Dictionary<MongoId, ObjectCache<Dictionary<string, Dictionary<MongoId, MongoId>>>> _originalTraderQuestAssorts = new();

        public TraderOfferSource(LoggingUtil loggingUtil, DatabaseService databaseService, Action restrictTraderOffersAction) : base()
        {
            _loggingUtil = loggingUtil;
            _databaseService = databaseService;

            _restrictTraderOffersAction = restrictTraderOffersAction;
        }

        protected override void OnUpdateCache()
        {
            foreach ((MongoId id, Trader trader) in _databaseService.GetTables().Traders)
            {
                if (id == Traders.FENCE)
                {
                    continue;
                }

                if (trader.Assort?.Items?.Count > 0)
                {
                    if (!_originalTraderAssorts.ContainsKey(id))
                    {
                        _originalTraderAssorts.Add(id, new());
                    }

                    _originalTraderAssorts[id].CacheValueAndThrowIfNull(trader.Assort);
                }

                if (trader.QuestAssort?.Count > 0)
                {
                    if (!_originalTraderQuestAssorts.ContainsKey(id))
                    {
                        _originalTraderQuestAssorts.Add(id, new());
                    }

                    _originalTraderQuestAssorts[id].CacheValueAndThrowIfNull(trader.QuestAssort);
                }
            }
        }

        protected override void OnRestoreCache()
        {
            foreach ((MongoId id, Trader trader) in _databaseService.GetTables().Traders)
            {
                if (_originalTraderAssorts.ContainsKey(id))
                {
                    trader.Assort = _originalTraderAssorts[id].GetValueAndThrowIfNull();
                }

                if (_originalTraderQuestAssorts.ContainsKey(id))
                {
                    Dictionary<string, Dictionary<MongoId, MongoId>> questAssortFromCache = _originalTraderQuestAssorts[id].GetValueAndThrowIfNull();
                    foreach (string key in questAssortFromCache.Keys)
                    {
                        if (trader.QuestAssort.ContainsKey(key))
                        {
                            trader.QuestAssort[key] = questAssortFromCache[key];
                        }
                        else
                        {
                            trader.QuestAssort.Add(key, questAssortFromCache[key]);
                        }
                    }
                }
            }
        }

        protected override void OnDisable()
        {
            _loggingUtil.Info("Restricting trader offers...");

            _restrictTraderOffersAction();
        }

        protected override void OnEnable()
        {
            _loggingUtil.Info("Restoring original trader offers...");
        }

        protected override void OnRefresh()
        {
            throw new NotImplementedException();
        }
    }
}
