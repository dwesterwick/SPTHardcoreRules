namespace HardcoreRules.Utils.TraderOfferSources.Internal
{
    public interface IOfferSource
    {
        public void Enable();
        public void Disable();
        public void Refresh();
    }
}
