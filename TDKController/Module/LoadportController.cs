using System;

namespace TDKController
{
    public class LoadportController : ILoadportController
    {
        private readonly LoadportControllerConfig _config;

        public LoadportController(LoadportControllerConfig config)
        {
            // Null-check per constitution constructor injection rules
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        // Implement ILoadportController interface members here
    }
}
