package alllogger.server.endpoint;


import org.apache.commons.lang.NullArgumentException;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import thesallab.foundation.exception.EmptyStringArgumentException;

import javax.websocket.*;
import javax.websocket.server.ServerEndpoint;

/**
 * Websocket服务端点。
 */
@ServerEndpoint(value = "/endpoint")
public class Endpoint {

    // **************** 公开变量

    // **************** 私有变量

    /**
     * Log4j logger。
     */
    private static Logger logger = LogManager.getLogger(Endpoint.class);

    // **************** 继承方法

    // **************** 公开方法

    @OnMessage
    public void onMessage(String message) {
        if (message == null) {
            throw new NullArgumentException("message");
        }

        if ("".equals(message)) {
            throw new EmptyStringArgumentException("message");
        }

        onMessageInterceptor().next(this, message);
    }

    // **************** 私有方法

    /**
     * 获得OnMessage拦截器。
     *
     * @return OnMessage拦截器。
     */
    private static OnMessageInterceptor onMessageInterceptor() {
        OnMessageInterceptor ret = new MessageLogInterceptor();
        ret.setNext(null);
        return ret;
    }

}
