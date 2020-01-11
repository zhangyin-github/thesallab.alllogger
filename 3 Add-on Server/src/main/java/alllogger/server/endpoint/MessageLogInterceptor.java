package alllogger.server.endpoint;

import alllogger.server.User;

import org.apache.commons.lang.NullArgumentException;
import thesallab.foundation.exception.EmptyStringArgumentException;

/**
 * 消息日志拦截器。
 */
public class MessageLogInterceptor extends OnMessageInterceptor {

    // **************** 公开变量

    // **************** 私有变量

    // **************** 继承方法

    /**
     * 执行预操作。
     *
     * @param endpoint 服务端点。
     * @param message  消息。
     * @return 是否执行下一拦截器。
     */
    @Override
    protected boolean doPre(Endpoint endpoint, String message) {
        if (endpoint == null) {
            throw new NullArgumentException("endpoint");
        }

        if (message == null) {
            throw new NullArgumentException("message");
        }

        if ("".equals(message)) {
            throw new EmptyStringArgumentException("message");
        }

        User.log(message);
        return true;
    }

    /**
     * 执行后操作。
     *
     * @param endpoint 服务端点。
     * @param message  消息。
     */
    @Override
    protected void doPost(Endpoint endpoint, String message) {
        if (endpoint == null) {
            throw new NullArgumentException("endpoint");
        }

        if (message == null) {
            throw new NullArgumentException("message");
        }

        if ("".equals(message)) {
            throw new EmptyStringArgumentException("message");
        }
    }

    // **************** 公开方法

    // **************** 私有方法

}
