using System.Web.Http;

namespace CQCopyPasteAdapter.WebApi;

public class QQCommandController : ApiController
{
    [HttpPost]
    public string Post([FromBody] QQSendCommand command)
    {
        // 处理请求并返回响应
        // ...
        return command.Message;
    }
}