namespace HardcoreRules.Utils.TraderOfferSources.Internal
{
    public abstract class AbstractOfferSource
    {
        private bool _cacheCreated = false;

        public AbstractOfferSource()
        {

        }

        protected abstract void OnUpdateCache();
        protected abstract void OnRestoreCache();

        protected abstract void OnEnable();
        public void Enable()
        {
            if (_cacheCreated)
            {
                OnRestoreCache();
            }

            OnEnable();
        }

        protected abstract void OnDisable();
        public void Disable()
        {
            OnUpdateCache();
            _cacheCreated = true;

            OnDisable();
        }

        protected abstract void OnRefresh();
        public void Refresh()
        {
            OnRefresh();
        }
    }
}
