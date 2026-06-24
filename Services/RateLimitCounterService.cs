namespace MilkbooksImageProcessor.Services
{
    public class RateLimitCounterService
    {
        public const int Limit = 50;
        private static readonly TimeSpan Window = TimeSpan.FromHours(1);

        private int _used = 0;
        private DateTime _windowStart = DateTime.UtcNow;
        private readonly object _lock = new();

        public int Remaining
        {
            get
            {
                lock (_lock)
                {
                    ResetIfExpired();
                    return Math.Max(0, Limit - _used);
                }
            }
        }

        public void Increment()
        {
            lock (_lock)
            {
                ResetIfExpired();
                _used++;
            }
        }

        private void ResetIfExpired()
        {
            if (DateTime.UtcNow - _windowStart >= Window)
            {
                _used = 0;
                _windowStart = DateTime.UtcNow;
            }
        }
    }
}
