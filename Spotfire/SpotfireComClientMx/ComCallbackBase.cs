
namespace Mobius.SpotfireClient.ComAutomation
{
	/// <summary>
	/// CallbackBase
	/// </summary>

	public class ComCallbackBase
    {
        public virtual void Exited(ExitContextWrapper context)
        {
        }

        public virtual void LoadViews(LoadViewsContextWrapper context)
        {
        }

        public virtual void Started(StartContextWrapper context)
        {
        }
    }
}
