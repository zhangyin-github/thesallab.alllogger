package alllogger.server;

import com.mongodb.client.MongoCollection;
import org.apache.commons.lang.NullArgumentException;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.bson.Document;
import thesallab.configuration.Config;
import thesallab.foundation.exception.EmptyStringArgumentException;
import thesallab.foundation.helper.MongoHelper;

/**
 * 用户类。
 *
 * @author Zhang, Yin
 */
public class User {

    // **************** 公开变量

    /**
     * 日志存储Mongodb服务器配置项键。
     */
    public static final String MONGODB_SERVERS =
            "alllogger.server.user" + ".mongodbservers";

    // **************** 私有变量

    /**
     * Log4j logger。
     */
    private static Logger logger = LogManager.getLogger(User.class);

    /**
     * 日志存储表。
     */
    private static MongoCollection<Document> logCollection =
            MongoHelper.clientFromMongodbServers(Config.getNotNull(MONGODB_SERVERS)).getDatabase("all_logger").getCollection("log");

    // **************** 继承方法

    // **************** 公开方法

    /**
     * 记录日志。
     *
     * @param log`日志。
     */
    public static void log(String log) {
        if (log == null) {
            throw new NullArgumentException("log");
        }

        if ("".equals(log)) {
            throw new EmptyStringArgumentException("log");
        }

        Document document = new Document();
        try {
            document.put("log", log);
        } catch (Exception e) {
            logger.error(e);
        }
        document.put("time", System.currentTimeMillis());
        logCollection.insertOne(document);
    }

    // **************** 私有方法

}
