using MsgPack.Serialization;
using System;

namespace Infrastructure.MsgPack
{
    public static class MsgPackHelper<T>
    {
        public static MessagePackSerializer<T> serializer;

        static MsgPackHelper()
        {
            var context = new SerializationContext { SerializationMethod = SerializationMethod.Map };
            serializer = MessagePackSerializer.Get<T>(context);
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static T UnPack(byte[] bytes) //解码函数
        {
            try
            {
                var deserializedObject = serializer.UnpackSingleObject(bytes);
                //serializer.PackSingleObject(deserializedObject);
                return deserializedObject;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw ex;
            }
            
        }

        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="bedinfo"></param>
        /// <returns></returns>
        public static byte[] Pack(T bedinfo)
        {
            try
            {
                return serializer.PackSingleObject(bedinfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
            
        }
    }
}
