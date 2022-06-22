using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using AlibabaCloud.TairSDK.TairSearch;
using AlibabaCloud.TairSDK.TairSearch.Param;
using NUnit.Framework;
using StackExchange.Redis;

namespace TestSearchTest
{
    public class SearchTests
    {
        private static readonly ConnectionMultiplexer connDC = ConnectionMultiplexer.Connect("localhost");
        private IDatabase db = connDC.GetDatabase(0);
        private readonly TairSearch tair = new(connDC, 0);

        [Test]
        public void tftcreateindex()
        {
            db.KeyDelete("tftkey");
            string ret = tair.tftcreateindex("tftkey",
                "{\"mappings\":{\"dynamic\":\"false\",\"properties\":{\"f0\":{\"type\":\"text\"},\"f1\":{\"type\":\"text\"}}}}");
            Assert.AreEqual("OK", ret);

            string mapping = tair.tftgetindexmappings("tftkey");
            Assert.AreEqual(
                "{\"tftkey\":{\"mappings\":{\"_source\":{\"enabled\":true,\"excludes\":[],\"includes\":[]},\"dynamic\":\"false\",\"properties\":{\"f0\":{\"boost\":1.0,\"enabled\":true,\"ignore_above\":-1,\"index\":true,\"similarity\":\"classic\",\"type\":\"text\"},\"f1\":{\"boost\":1.0,\"enabled\":true,\"ignore_above\":-1,\"index\":true,\"similarity\":\"classic\",\"type\":\"text\"}}}}}",
                mapping);
        }

        [Test]
        public void tftupdateindex()
        {
            db.KeyDelete("tftkey");
            string ret = tair.tftcreateindex("tftkey",
                "{\"mappings\":{\"dynamic\":\"false\",\"properties\":{\"f0\":{\"type\":\"text\"}}}}");
            Assert.AreEqual("OK", ret);

            string mapping = tair.tftgetindexmappings("tftkey");
            Assert.AreEqual(
                "{\"tftkey\":{\"mappings\":{\"_source\":{\"enabled\":true,\"excludes\":[],\"includes\":[]},\"dynamic\":\"false\",\"properties\":{\"f0\":{\"boost\":1.0,\"enabled\":true,\"ignore_above\":-1,\"index\":true,\"similarity\":\"classic\",\"type\":\"text\"}}}}}",
                mapping);

            ret = tair.tftupdateindex("tftkey", "{\"mappings\":{\"properties\":{\"f1\":{\"type\":\"text\"}}}}");
            Assert.AreEqual("OK", ret);

            mapping = tair.tftgetindexmappings("tftkey");
            Assert.AreEqual(
                "{\"tftkey\":{\"mappings\":{\"_source\":{\"enabled\":true,\"excludes\":[],\"includes\":[]},\"dynamic\":\"false\",\"properties\":{\"f0\":{\"boost\":1.0,\"enabled\":true,\"ignore_above\":-1,\"index\":true,\"similarity\":\"classic\",\"type\":\"text\"},\"f1\":{\"boost\":1.0,\"enabled\":true,\"ignore_above\":-1,\"index\":true,\"similarity\":\"classic\",\"type\":\"text\"}}}}}",
                mapping);
        }

        [Test]
        public void tftadddoc()
        {
            db.KeyDelete("tftkey");
            tair.tftcreateindex("tftkey",
                "{\"mappings\":{\"dynamic\":\"false\",\"properties\":{\"f0\":{\"type\":\"text\"},\"f1\":{\"type\":\"text\"}}}}");
            tair.tftadddoc("tftkey", "{\"f0\":\"v0\",\"f1\":\"3\"}", "1");
            tair.tftadddoc("tftkey", "{\"f0\":\"v1\",\"f1\":\"3\"}", "2");
            tair.tftadddoc("tftkey", "{\"f0\":\"v3\",\"f1\":\"3\"}", "3");
            tair.tftadddoc("tftkey", "{\"f0\":\"v3\",\"f1\":\"4\"}", "4");
            tair.tftadddoc("tftkey", "{\"f0\":\"v3\",\"f1\":\"5\"}", "5");

            Assert.AreEqual(
                "{\"hits\":{\"hits\":[{\"_id\":\"1\",\"_index\":\"tftkey\",\"_score\":1.223144,\"_source\":{\"f0\":\"v0\",\"f1\":\"3\"}},{\"_id\":\"2\",\"_index\":\"tftkey\",\"_score\":1.223144,\"_source\":{\"f0\":\"v1\",\"f1\":\"3\"}},{\"_id\":\"3\",\"_index\":\"tftkey\",\"_score\":1.223144,\"_source\":{\"f0\":\"v3\",\"f1\":\"3\"}}],\"max_score\":1.223144,\"total\":{\"relation\":\"eq\",\"value\":3}}}",
                tair.tftsearch("tftkey", "{\"query\":{\"match\":{\"f1\":\"3\"}}}"));

            Assert.AreEqual("{\"_id\":\"3\",\"_source\":{\"f0\":\"v3\",\"f1\":\"3\"}}", tair.tftgetdoc("tftkey", "3"));
            Assert.AreEqual("1", tair.tftdeldoc("tftkey", "3"));
            Assert.AreEqual(null, tair.tftgetdoc("tftkey", "3"));

            Assert.AreEqual(
                "{\"tftkey\":{\"mappings\":{\"_source\":{\"enabled\":true,\"excludes\":[],\"includes\":[]},\"dynamic\":\"false\",\"properties\":{\"f0\":{\"boost\":1.0,\"enabled\":true,\"ignore_above\":-1,\"index\":true,\"similarity\":\"classic\",\"type\":\"text\"},\"f1\":{\"boost\":1.0,\"enabled\":true,\"ignore_above\":-1,\"index\":true,\"similarity\":\"classic\",\"type\":\"text\"}}}}}",
                tair.tftgetindexmappings("tftkey"));
        }

        [Test]
        public void tftupdatedocfield()
        {
            db.KeyDelete("tftkey");
            string ret = tair.tftcreateindex("tftkey",
                "{\"mappings\":{\"dynamic\":\"false\",\"properties\":{\"f0\":{\"type\":\"text\"}}}}");
            Assert.AreEqual(ret, "OK");

            tair.tftadddoc("tftkey", "{\"f0\":\"redis is a nosql database\"}", "1");
            Assert.AreEqual(
                "{\"hits\":{\"hits\":[{\"_id\":\"1\",\"_index\":\"tftkey\",\"_score\":0.153426,\"_source\":{\"f0\":\"redis is a nosql database\"}}],\"max_score\":0.153426,\"total\":{\"relation\":\"eq\",\"value\":1}}}",
                tair.tftsearch("tftkey", "{\"query\":{\"term\":{\"f0\":\"redis\"}}}"));

            ret = tair.tftupdateindex("tftkey", "{\"mappings\":{\"properties\":{\"f1\":{\"type\":\"text\"}}}}");
            Assert.AreEqual(ret, "OK");

            tair.tftupdatedocfield("tftkey", "1", "{\"f1\":\"mysql is a dbms\"}");
            Assert.AreEqual(
                "{\"hits\":{\"hits\":[{\"_id\":\"1\",\"_index\":\"tftkey\",\"_score\":0.191783,\"_source\":{\"f0\":\"redis is a nosql database\",\"f1\":\"mysql is a dbms\"}}],\"max_score\":0.191783,\"total\":{\"relation\":\"eq\",\"value\":1}}}",
                tair.tftsearch("tftkey", "{\"query\":{\"term\":{\"f1\":\"mysql\"}}}"));
        }

        [Test]
        public void tftincrlongdocfield()
        {
            db.KeyDelete("tftkey");
            try
            {
                tair.tftincrlongdocfield("tftkey", "1", "f0", 1);
            }
            catch (Exception e)
            {
                Assert.AreEqual("ERR index [tftkey] not exists", e.Message);
            }

            string ret = tair.tftcreateindex("tftkey",
                "{\"mappings\":{\"dynamic\":\"false\",\"properties\":{\"f0\":{\"type\":\"text\"}}}}");
            Assert.AreEqual(ret, "OK");
            try
            {
                tair.tftincrlongdocfield("tftkey", "1", "f0", 1);
            }
            catch (Exception e)
            {
                Assert.AreEqual("ERR failed to parse field [f0]", e.Message);
            }

            db.KeyDelete("tftkey");
            ret = tair.tftcreateindex("tftkey",
                "{\"mappings\":{\"dynamic\":\"false\",\"properties\":{\"f0\":{\"type\":\"long\"}}}}");
            Assert.AreEqual(ret, "OK");

            Assert.AreEqual(1, tair.tftincrlongdocfield("tftkey", "1", "f0", 1));
            Assert.AreEqual(0, tair.tftincrlongdocfield("tftkey", "1", "f0", -1));
            Assert.AreEqual(1, tair.tftexists("tftkey", "1"));
        }

        [Test]
        public void tftincrfloatdocfield()
        {
            db.KeyDelete("tftkey");
            try
            {
                tair.tftincrfloatdocfield("tftkey", "1", "f0", 1.1);
            }
            catch (Exception e)
            {
                Assert.AreEqual("ERR index [tftkey] not exists", e.Message);
            }

            String ret = tair.tftcreateindex("tftkey",
                "{\"mappings\":{\"dynamic\":\"false\",\"properties\":{\"f0\":{\"type\":\"text\"}}}}");
            Assert.AreEqual(ret, "OK");
            try
            {
                tair.tftincrfloatdocfield("tftkey", "1", "f0", 1.1);
            }
            catch (Exception e)
            {
                Assert.AreEqual("ERR failed to parse field [f0]", e.Message);
            }

            db.KeyDelete("tftkey");
            ret = tair.tftcreateindex("tftkey",
                "{\"mappings\":{\"dynamic\":\"false\",\"properties\":{\"f0\":{\"type\":\"double\"}}}}");
            Assert.AreEqual(ret, "OK");
            double value = tair.tftincrfloatdocfield("tftkey", "1", "f0", 1.1);
            Assert.AreEqual(1.1d, value);
            value = tair.tftincrfloatdocfield("tftkey", "1", "f0", -1.1);
            Assert.AreEqual(0d, value);
            Assert.AreEqual(1, tair.tftexists("tftkey", "1"));
        }

        [Test]
        public void tftdeldocfield()
        {
            db.KeyDelete("tftkey");
            Assert.AreEqual(0, tair.tftdeldocfield("tffkey", "1", "f0"));

            string ret = tair.tftcreateindex("tftkey",
                "{\"mappings\":{\"dynamic\":\"false\",\"properties\":{\"f0\":{\"type\":\"long\"}}}}");
            Assert.AreEqual("OK", ret);
            tair.tftincrlongdocfield("tftkey", "1", "f0", 1);
            tair.tftincrfloatdocfield("tftkey", "1", "f1", 1.1);
            Assert.AreEqual(2, tair.tftdeldocfield("tftkey", "1", "f0", "f1", "f2"));
        }

        [Test]
        public void tftdeldoc()
        {
            db.KeyDelete("tftkey");
            tair.tftcreateindex("tftkey",
                "{\"mappings\":{\"dynamic\":\"false\",\"properties\":{\"f0\":{\"type\":\"text\"},\"f1\":{\"type\":\"text\"}}}}");
            tair.tftadddoc("tftkey", "{\"f0\":\"v0\",\"f1\":\"3\"}", "1");
            tair.tftadddoc("tftkey", "{\"f0\":\"v1\",\"f1\":\"3\"}", "2");
            tair.tftadddoc("tftkey", "{\"f0\":\"v3\",\"f1\":\"3\"}", "3");
            tair.tftadddoc("tftkey", "{\"f0\":\"v3\",\"f1\":\"4\"}", "4");
            tair.tftadddoc("tftkey", "{\"f0\":\"v3\",\"f1\":\"5\"}", "5");

            Assert.AreEqual(1, tair.tftexists("tftkey", "3"));
            Assert.AreEqual(5, tair.tftdocnum("tftkey"));
            Assert.AreEqual("3", tair.tftdeldoc("tftkey", "3", "4", "5"));
            Assert.AreEqual(0, tair.tftexists("tftkey", "3"));
            Assert.AreEqual(2, tair.tftdocnum("tftkey"));
        }

        [Test]
        public void tftdelall()
        {
            db.KeyDelete("tftkey");
            tair.tftcreateindex("tftkey",
                "{\"mappings\":{\"dynamic\":\"false\",\"properties\":{\"f0\":{\"type\":\"text\"},\"f1\":{\"type\":\"text\"}}}}");
            tair.tftadddoc("tftkey", "{\"f0\":\"v0\",\"f1\":\"3\"}", "1");
            tair.tftadddoc("tftkey", "{\"f0\":\"v1\",\"f1\":\"3\"}", "2");
            tair.tftadddoc("tftkey", "{\"f0\":\"v3\",\"f1\":\"3\"}", "3");
            tair.tftadddoc("tftkey", "{\"f0\":\"v3\",\"f1\":\"4\"}", "4");
            tair.tftadddoc("tftkey", "{\"f0\":\"v3\",\"f1\":\"5\"}", "5");

            Assert.AreEqual("OK", tair.tftdelall("tftkey"));
            Assert.AreEqual(0, tair.tftdocnum("tftkey"));
        }

        [Test]
        public void tftscandocid()
        {
            db.KeyDelete("tftkey");
            tair.tftcreateindex("tftkey",
                "{\"mappings\":{\"dynamic\":\"false\",\"properties\":{\"f0\":{\"type\":\"text\"},\"f1\":{\"type\":\"text\"}}}}");
            tair.tftadddoc("tftkey", "{\"f0\":\"v0\",\"f1\":\"3\"}", "1");
            tair.tftadddoc("tftkey", "{\"f0\":\"v1\",\"f1\":\"3\"}", "2");
            tair.tftadddoc("tftkey", "{\"f0\":\"v3\",\"f1\":\"3\"}", "3");
            tair.tftadddoc("tftkey", "{\"f0\":\"v3\",\"f1\":\"4\"}", "4");
            tair.tftadddoc("tftkey", "{\"f0\":\"v3\",\"f1\":\"5\"}", "5");
            TFTScanResult<String> res = tair.tftscandocid("tftkey", "0");
            Assert.AreEqual(0.ToString(), res.getCursor());
            Assert.True(res.getResult().Count == 5);

            Assert.AreEqual("1", res.getResult()[0]);
            Assert.AreEqual("2", res.getResult()[1]);
        }

        [Test]
        public void tftscandocidwithcount()
        {
            db.KeyDelete("tftkey");
            tair.tftcreateindex("tftkey",
                "{\"mappings\":{\"dynamic\":\"false\",\"properties\":{\"f0\":{\"type\":\"text\"},\"f1\":{\"type\":\"text\"}}}}");
            tair.tftadddoc("tftkey", "{\"f0\":\"v0\",\"f1\":\"3\"}", "1");
            tair.tftadddoc("tftkey", "{\"f0\":\"v1\",\"f1\":\"3\"}", "2");
            tair.tftadddoc("tftkey", "{\"f0\":\"v3\",\"f1\":\"3\"}", "3");
            tair.tftadddoc("tftkey", "{\"f0\":\"v3\",\"f1\":\"4\"}", "4");
            tair.tftadddoc("tftkey", "{\"f0\":\"v3\",\"f1\":\"5\"}", "5");

            TFTScanParams param = new TFTScanParams();
            param.count(3);
            TFTScanResult<String> res = tair.tftscandocid("tftkey", "0", param);
            Assert.AreEqual("3", res.getCursor());
            Assert.True(res.getResult().Count == 3);

            Assert.AreEqual("1", res.getResult()[0]);
            Assert.AreEqual("2", res.getResult()[1]);
            Assert.AreEqual("3", res.getResult()[2]);

            TFTScanResult<String> res2 = tair.tftscandocid("tftkey", "3", param);
            Assert.AreEqual("0", res2.getCursor());
            Assert.True(res2.getResult().Count == 2);

            Assert.AreEqual("4", res2.getResult()[0]);
            Assert.AreEqual("5", res2.getResult()[1]);
        }

        [Test]
        public void tftscandocidwithmatch()
        {
            db.KeyDelete("tftkey");

            tair.tftcreateindex("tftkey",
                "{\"mappings\":{\"dynamic\":\"false\",\"properties\":{\"f0\":{\"type\":\"text\"},\"f1\":{\"type\":\"text\"}}}}");
            tair.tftadddoc("tftkey", "{\"f0\":\"v0\",\"f1\":\"3\"}", "1_redis_doc");
            tair.tftadddoc("tftkey", "{\"f0\":\"v1\",\"f1\":\"3\"}", "2_redis_doc");
            tair.tftadddoc("tftkey", "{\"f0\":\"v3\",\"f1\":\"3\"}", "3_mysql_doc");
            tair.tftadddoc("tftkey", "{\"f0\":\"v3\",\"f1\":\"4\"}", "4_mysql_doc");
            tair.tftadddoc("tftkey", "{\"f0\":\"v3\",\"f1\":\"5\"}", "5_tidb_doc");

            TFTScanParams param = new TFTScanParams();
            param.match("*redis*");
            TFTScanResult<String> res = tair.tftscandocid("tftkey", "0", param);
            Assert.AreEqual("0", res.getCursor());
            Assert.True(res.getResult().Count == 2);

            Assert.AreEqual("1_redis_doc", res.getResult()[0]);
            Assert.AreEqual("2_redis_doc", res.getResult()[1]);

            TFTScanParams params2 = new TFTScanParams();
            params2.match("*tidb*");
            TFTScanResult<String> res2 = tair.tftscandocid("tftkey", "0", params2);
            Assert.AreEqual("0", res2.getCursor());
            Assert.True(res2.getResult().Count == 1);

            Assert.AreEqual("5_tidb_doc", res2.getResult()[0]);
        }

        [Test]
        public void unicodetest()
        {
            db.KeyDelete("tftkey");
            tair.tftmappingindex("tftkey",
                "{\"mappings\":{\"properties\":{\"f0\":{\"type\":\"text\",\"analyzer\":\"chinese\"}}}}");
            Assert.AreEqual(
                "{\"tftkey\":{\"mappings\":{\"_source\":{\"enabled\":true,\"excludes\":[],\"includes\":[]},\"dynamic\":\"false\",\"properties\":{\"f0\":{\"analyzer\":\"chinese\",\"boost\":1.0,\"enabled\":true,\"ignore_above\":-1,\"index\":true,\"similarity\":\"classic\",\"type\":\"text\"}}}}}",
                tair.tftgetindexmappings("tftkey"));

            db.KeyDelete("tftkey");
            tair.tftmappingindex("tftkey",
                "{\"mappings\":{\"properties\":{\"f0\":{\"type\":\"text\",\"search_analyzer\":\"chinese\"}}}}");
            Assert.AreEqual(
                "{\"tftkey\":{\"mappings\":{\"_source\":{\"enabled\":true,\"excludes\":[],\"includes\":[]},\"dynamic\":\"false\",\"properties\":{\"f0\":{\"boost\":1.0,\"enabled\":true,\"ignore_above\":-1,\"index\":true,\"similarity\":\"classic\",\"type\":\"text\",\"search_analyzer\":\"chinese\"}}}}}",
                tair.tftgetindexmappings("tftkey"));
            db.KeyDelete("tftkey");

            tair.tftmappingindex("tftkey",
                "{\"mappings\":{\"properties\":{\"f0\":{\"type\":\"text\",\"analyzer\":\"chinese\", \"search_analyzer\":\"chinese\"}}}}");
            Assert.AreEqual(
                "{\"tftkey\":{\"mappings\":{\"_source\":{\"enabled\":true,\"excludes\":[],\"includes\":[]},\"dynamic\":\"false\",\"properties\":{\"f0\":{\"analyzer\":\"chinese\",\"boost\":1.0,\"enabled\":true,\"ignore_above\":-1,\"index\":true,\"similarity\":\"classic\",\"type\":\"text\",\"search_analyzer\":\"chinese\"}}}}}",
                tair.tftgetindexmappings("tftkey"));
            tair.tftadddoc("tftkey", "{\"f0\":\"夏天是一个很热的季节\"}", "1");
            Assert.AreEqual(
                "{\"hits\":{\"hits\":[{\"_id\":\"1\",\"_index\":\"tftkey\",\"_score\":0.077948,\"_source\":{\"f0\":\"夏天是一个很热的季节\"}}],\"max_score\":0.077948,\"total\":{\"relation\":\"eq\",\"value\":1}}}",
                tair.tftsearch("tftkey", "{\"query\":{\"match\":{\"f0\":\"夏天冬天\"}}}"));
        }

        [Test]
        public void searchchcachetest()
        {
            db.KeyDelete("tftkey");
            tair.tftmappingindex("tftkey",
                "{\"mappings\":{\"dynamic\":\"false\",\"properties\":{\"f0\":{\"type\":\"text\"},\"f1\":{\"type\":\"text\"}}}}");
            tair.tftadddoc("tftkey", "{\"f0\":\"v0\",\"f1\":\"3\"}", "1");
            tair.tftadddoc("tftkey", "{\"f0\":\"v1\",\"f1\":\"3\"}", "2");

            Assert.AreEqual(
                "{\"hits\":{\"hits\":[{\"_id\":\"1\",\"_index\":\"tftkey\",\"_score\":0.594535,\"_source\":{\"f0\":\"v0\",\"f1\":\"3\"}},{\"_id\":\"2\",\"_index\":\"tftkey\",\"_score\":0.594535,\"_source\":{\"f0\":\"v1\",\"f1\":\"3\"}}],\"max_score\":0.594535,\"total\":{\"relation\":\"eq\",\"value\":2}}}",
                tair.tftsearch("tftkey", "{\"query\":{\"match\":{\"f1\":\"3\"}}}", true));

            tair.tftadddoc("tftkey", "{\"f0\":\"v3\",\"f1\":\"3\"}", "3");
            tair.tftadddoc("tftkey", "{\"f0\":\"v3\",\"f1\":\"4\"}", "4");
            tair.tftadddoc("tftkey", "{\"f0\":\"v3\",\"f1\":\"5\"}", "5");

            Assert.AreEqual(
                "{\"hits\":{\"hits\":[{\"_id\":\"1\",\"_index\":\"tftkey\",\"_score\":0.594535,\"_source\":{\"f0\":\"v0\",\"f1\":\"3\"}},{\"_id\":\"2\",\"_index\":\"tftkey\",\"_score\":0.594535,\"_source\":{\"f0\":\"v1\",\"f1\":\"3\"}}],\"max_score\":0.594535,\"total\":{\"relation\":\"eq\",\"value\":2}}}",
                tair.tftsearch("tftkey", "{\"query\":{\"match\":{\"f1\":\"3\"}}}", true));

            Assert.AreEqual(
                "{\"hits\":{\"hits\":[{\"_id\":\"1\",\"_index\":\"tftkey\",\"_score\":1.223144,\"_source\":{\"f0\":\"v0\",\"f1\":\"3\"}},{\"_id\":\"2\",\"_index\":\"tftkey\",\"_score\":1.223144,\"_source\":{\"f0\":\"v1\",\"f1\":\"3\"}},{\"_id\":\"3\",\"_index\":\"tftkey\",\"_score\":1.223144,\"_source\":{\"f0\":\"v3\",\"f1\":\"3\"}}],\"max_score\":1.223144,\"total\":{\"relation\":\"eq\",\"value\":3}}}",
                tair.tftsearch("tftkey", "{\"query\":{\"match\":{\"f1\":\"3\"}}}"));

            // wait for LRU cache expired
            Thread.Sleep(10000);
            Assert.AreEqual(
                "{\"hits\":{\"hits\":[{\"_id\":\"1\",\"_index\":\"tftkey\",\"_score\":1.223144,\"_source\":{\"f0\":\"v0\",\"f1\":\"3\"}},{\"_id\":\"2\",\"_index\":\"tftkey\",\"_score\":1.223144,\"_source\":{\"f0\":\"v1\",\"f1\":\"3\"}},{\"_id\":\"3\",\"_index\":\"tftkey\",\"_score\":1.223144,\"_source\":{\"f0\":\"v3\",\"f1\":\"3\"}}],\"max_score\":1.223144,\"total\":{\"relation\":\"eq\",\"value\":3}}}",
                tair.tftsearch("tftkey", "{\"query\":{\"match\":{\"f1\":\"3\"}}}", true));
        }

        [Test]
        public void tftmaddteststring()
        {
            db.KeyDelete("tftkey");
            tair.tftmappingindex("tftkey",
                "{\"mappings\":{\"dynamic\":\"false\",\"properties\":{\"f0\":{\"type\":\"text\"},\"f1\":{\"type\":\"text\"}}}}");
            Dictionary<String, String> docs = new Dictionary<string, string>();
            docs.Add("{\"f0\":\"v0\",\"f1\":\"3\"}", "1");
            docs.Add("{\"f0\":\"v1\",\"f1\":\"3\"}", "2");
            docs.Add("{\"f0\":\"v3\",\"f1\":\"3\"}", "3");
            docs.Add("{\"f0\":\"v3\",\"f1\":\"4\"}", "4");
            docs.Add("{\"f0\":\"v3\",\"f1\":\"5\"}", "5");

            tair.tftmadddoc("tftkey", docs);

            Assert.AreEqual(
                "{\"hits\":{\"hits\":[{\"_id\":\"1\",\"_index\":\"tftkey\",\"_score\":1.223144,\"_source\":{\"f0\":\"v0\",\"f1\":\"3\"}},{\"_id\":\"2\",\"_index\":\"tftkey\",\"_score\":1.223144,\"_source\":{\"f0\":\"v1\",\"f1\":\"3\"}},{\"_id\":\"3\",\"_index\":\"tftkey\",\"_score\":1.223144,\"_source\":{\"f0\":\"v3\",\"f1\":\"3\"}}],\"max_score\":1.223144,\"total\":{\"relation\":\"eq\",\"value\":3}}}",
                tair.tftsearch("tftkey", "{\"query\":{\"match\":{\"f1\":\"3\"}}}"));

            Assert.AreEqual("{\"_id\":\"3\",\"_source\":{\"f0\":\"v3\",\"f1\":\"3\"}}", tair.tftgetdoc("tftkey", "3"));
            Assert.AreEqual("1", tair.tftdeldoc("tftkey", "3"));
            Assert.AreEqual(null, tair.tftgetdoc("tftkey", "3"));

            Assert.AreEqual(
                "{\"tftkey\":{\"mappings\":{\"_source\":{\"enabled\":true,\"excludes\":[],\"includes\":[]},\"dynamic\":\"false\",\"properties\":{\"f0\":{\"boost\":1.0,\"enabled\":true,\"ignore_above\":-1,\"index\":true,\"similarity\":\"classic\",\"type\":\"text\"},\"f1\":{\"boost\":1.0,\"enabled\":true,\"ignore_above\":-1,\"index\":true,\"similarity\":\"classic\",\"type\":\"text\"}}}}}",
                tair.tftgetindexmappings("tftkey"));
        }

        [Test]
        public void tftmaddtestbyte()
        {
            db.KeyDelete("tftkey");
            tair.tftmappingindex("tftkey",
                "{\"mappings\":{\"dynamic\":\"false\",\"properties\":{\"f0\":{\"type\":\"text\"},\"f1\":{\"type\":\"text\"}}}}");
            Dictionary<byte[], byte[]> docs = new Dictionary<byte[], byte[]>();
            docs.Add(Encoding.UTF8.GetBytes("{\"f0\":\"v0\",\"f1\":\"3\"}"), Encoding.UTF8.GetBytes("1"));
            docs.Add(Encoding.UTF8.GetBytes("{\"f0\":\"v1\",\"f1\":\"3\"}"), Encoding.UTF8.GetBytes("2"));
            docs.Add(Encoding.UTF8.GetBytes("{\"f0\":\"v3\",\"f1\":\"3\"}"), Encoding.UTF8.GetBytes("3"));
            docs.Add(Encoding.UTF8.GetBytes("{\"f0\":\"v3\",\"f1\":\"4\"}"), Encoding.UTF8.GetBytes("4"));
            docs.Add(Encoding.UTF8.GetBytes("{\"f0\":\"v3\",\"f1\":\"5\"}"), Encoding.UTF8.GetBytes("5"));

            tair.tftmadddoc(Encoding.UTF8.GetBytes("tftkey"), docs);

            Assert.AreEqual(
                "{\"hits\":{\"hits\":[{\"_id\":\"1\",\"_index\":\"tftkey\",\"_score\":1.223144,\"_source\":{\"f0\":\"v0\",\"f1\":\"3\"}},{\"_id\":\"2\",\"_index\":\"tftkey\",\"_score\":1.223144,\"_source\":{\"f0\":\"v1\",\"f1\":\"3\"}},{\"_id\":\"3\",\"_index\":\"tftkey\",\"_score\":1.223144,\"_source\":{\"f0\":\"v3\",\"f1\":\"3\"}}],\"max_score\":1.223144,\"total\":{\"relation\":\"eq\",\"value\":3}}}",
                tair.tftsearch("tftkey", "{\"query\":{\"match\":{\"f1\":\"3\"}}}"));

            Assert.AreEqual("{\"_id\":\"3\",\"_source\":{\"f0\":\"v3\",\"f1\":\"3\"}}", tair.tftgetdoc("tftkey", "3"));
            Assert.AreEqual("1", tair.tftdeldoc("tftkey", "3"));
            Assert.AreEqual(null, tair.tftgetdoc("tftkey", "3"));

            Assert.AreEqual(
                "{\"tftkey\":{\"mappings\":{\"_source\":{\"enabled\":true,\"excludes\":[],\"includes\":[]},\"dynamic\":\"false\",\"properties\":{\"f0\":{\"boost\":1.0,\"enabled\":true,\"ignore_above\":-1,\"index\":true,\"similarity\":\"classic\",\"type\":\"text\"},\"f1\":{\"boost\":1.0,\"enabled\":true,\"ignore_above\":-1,\"index\":true,\"similarity\":\"classic\",\"type\":\"text\"}}}}}",
                tair.tftgetindexmappings("tftkey"));
        }
    }
}