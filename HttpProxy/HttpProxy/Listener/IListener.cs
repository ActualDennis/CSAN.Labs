using System;
using System.Collections.Generic;
using System.Text;

namespace HttpProxy.Listener {
    public interface IListener<T> {
        void Listen();

        event EventHandler<T> OnNewRequestReceived;
    }
}
