using System;
using System.Collections.Generic;
using System.Text;

namespace http_filetransfer {
    public interface IListener<T> {
        void Listen();

        event EventHandler<T> OnNewRequestReceived;
    }
}
