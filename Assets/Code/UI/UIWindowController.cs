using System;
using VContainer.Unity;

namespace Code.UI
{
    public abstract class UIWindowController<TView> : IStartable, IDisposable where TView : UIWindowView
    {
        protected TView view;
        
        protected UIWindowController(TView view)
        {
            this.view = view;
        }

        public virtual void Start()
        {
            onSubscribe();
        }

        public void Dispose()
        {
            onDispose();
        }

        protected abstract void onSubscribe();

        protected abstract void onDispose();
    }
}