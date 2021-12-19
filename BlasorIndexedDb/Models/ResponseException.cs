using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorIndexedDb.Models
{
    /// <summary>
    /// Manage own exceptions
    /// </summary>
    public class ResponseException : Exception
    {
        /// <summary>
        /// Command throw the exception
        /// </summary>
        public string Command { get; set; }
        /// <summary>
        /// Store name throw the exception
        /// </summary>
        public string StoreName { get; set; }
        /// <summary>
        /// Data inside the transaction if have
        /// </summary>
        public string TransactionData { get; set; }
        /// <summary>
        /// Basic constructor
        /// </summary>
        public ResponseException():base() { }
        /// <summary>
        /// Constructor with message
        /// </summary>
        public ResponseException(string message):base(message) { }
        /// <summary>
        /// Contructur with message and exception
        /// </summary>
        public ResponseException(string message, Exception innerException):base(message, innerException) { }

        /// <summary>
        /// Basic internal constructor
        /// </summary>
        /// <param name="command"></param>
        /// <param name="storeName"></param>
        /// <param name="data"></param>
        public ResponseException(string command, string storeName, string data) : base($"[{command}] {storeName}") =>
            (Command, StoreName, TransactionData) = (command, storeName, data);
        /// <summary>
        /// Constructor with the inner Exception
        /// </summary>
        /// <param name="command"></param>
        /// <param name="storeName"></param>
        /// <param name="data"></param>
        /// <param name="innerException"></param>
        public ResponseException(string command, string storeName, string data, Exception innerException): base($"[{command}] {storeName}", innerException) =>
            (Command, StoreName, TransactionData) = (command, storeName, data);
        /// <summary>
        /// Return error formated
        /// </summary>
        /// <returns></returns>
        public override string ToString() =>
            $"{this.Message} {(string.IsNullOrEmpty(this.TransactionData) ? "" : $"({this.TransactionData})")}";
    }
}
