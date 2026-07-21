using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Linq;

namespace TaxServiceAndApp.Models.SharedDTO
{
    [DataContract]
    public class DTOServerInfo
    {

        [DataMember]
        public string CustomerName { get; set; }
        public string MEMORY_ID { get; set; }
        public string PASSWORD { get; set; }
        public string LIBRARY_PATH { get; set; }
        public string CertificateName { get; set; }
        public string TokenType { get; set; }
        public string ServiceType { get; set; }
        public string DataBaseType { get; set; }


        [DataMember] public string API_URL { get; set; }
        [DataMember] public string FILE_KEY_PRIVATE { get; set; }
        [DataMember] public string FILE_CERTIFICATE { get; set; }
        [DataMember] public string TINS { get; set; }
        [DataMember]
        public string DBConn
        {
            get
            {
                try
                {
                    string con = string.
                        Format("Server={0};Database={1};user id ={2};password={3}; TrustServerCertificate=True;Connection Timeout={4};",
                        DataBaseServer, DbName, UserId, UserPass, ConnectionTimeOut);
                    return con;
                }
                catch 
                {
                    return "Error On Creating ConnectionString";
                }
            }
        }
        [DataMember]
        public string DBConnForModel
        {
            get
            {
                try
                {
                    string con = string.
                        Format("data source={0};initial catalog={1};persist security info=True;user id={2};password={3};multipleactiveresultsets=True;encrypt=True;trustservercertificate=True;application name=EntityFramework",
                        DataBaseServer, DbName, UserId, UserPass);
                    return con;
                }
                catch 
                {
                    return "Error On Creating ConnectionString";
                }
            }
        }
        [DataMember] public string DataBaseServer { get; set; }
        [DataMember] public string UserPass { get; set; }
        [DataMember] public string UserId { get; set; }
        [DataMember] public string DbName { get; set; }
        /// <summary>
        /// Based On Second
        /// </summary>
        [DataMember] public int ConnectionTimeOut { get; set; }
        [DataMember] public int StoreId { get; set; }


    }
}