package alllogger.server.endpoint;


import org.apache.commons.lang.NullArgumentException;
import thesallab.foundation.exception.EmptyStringArgumentException;

/**
 * OnMessage拦截器。
 *
 * @author Zhang, Yin
 */
public abstract class OnMessageInterceptor {

    // **************** 公开变量

    // **************** 私有变量

    /**
     * 下一OnMessage拦截器。
     */
    private OnMessageInterceptor next = null;

    // **************** 继承方法

    // **************** 公开方法

    /**
     * 执行下一拦截器。
     *
     * @param endpoint 服务端点。
     * @param message  消息。
     */
    public void next(Endpoint endpoint, String message) {
        if (endpoint == null) {
            throw new NullArgumentException("endpoint");
        }

        if (message == null) {
            throw new NullArgumentException("message");
        }

        if ("".equals(message)) {
            throw new EmptyStringArgumentException("message");
        }

        if (doPre(endpoint, message) && next != null) {
            next.next(endpoint, message);
        }
        doPost(endpoint, message);
    }

    /**
     * 执行预操作。
     *
     * @param endpoint 服务端点。
     * @param message  消息。
     * @return 是否执行下一拦截器。
     */
    protected abstract boolean doPre(Endpoint endpoint, String message);

    /**
     * 执行后操作。
     *
     * @param endpoint 服务端点。
     * @param message  消息。
     */
    protected abstract void doPost(Endpoint endpoint, String message);

    /**
     * 设置下一OnMessage拦截器。
     *
     * @param next 下一OnMessage拦截器。
     */
    public void setNext(OnMessageInterceptor next) {
        this.next = next;
    }

    // **************** 私有方法

}
