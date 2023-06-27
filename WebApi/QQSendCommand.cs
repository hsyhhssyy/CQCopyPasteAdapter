using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CQCopyPasteAdapter.WebApi
{
    public class QQSendCommand
    {
        public String ChannelId { get; set; }
        public String Message { get; set; }
    }
}
