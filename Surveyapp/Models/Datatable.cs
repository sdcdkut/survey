using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace Surveyapp.Models
{
      public class Datatable
    {
        public class DtResult<T>
        {
            [JsonProperty("draw")] public int Draw { get; set; }
            [JsonProperty("recordsTotal")] public int RecordsTotal { get; set; }
            [JsonProperty("recordsFiltered")] public int RecordsFiltered { get; set; }
            [JsonProperty("data")] public IEnumerable<T> Data { get; set; }

            [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
            public string Error { get; set; }

            public string PartialView { get; set; }
        }

        public abstract class DtRow
        {
            [JsonProperty("DT_RowId")] public virtual string DtRowId => null;
            [JsonProperty("DT_RowClass")] public virtual string DtRowClass => null;
            [JsonProperty("DT_RowData")] public virtual object DtRowData => null;
            [JsonProperty("DT_RowAttr")] public virtual object DtRowAttr => null;
        }

        public class DtParameters
        {
            public int Draw { get; set; }
            public DtColumn[] Columns { get; set; }
            public DtOrder[] Order { get; set; }
            public int Start { get; set; }
            public int Length { get; set; }
            public DtSearch Search { get; set; }

            public string SortOrder => Columns != null && Order != null && Order.Length > 0
                ? (Columns[Order[0].Column].Data +
                   (Order[0].Dir == DtOrderDir.Desc ? " " + Order[0].Dir : string.Empty))
                : null;

            public IEnumerable<string> AdditionalValues { get; set; }
        }

        public class DtColumn
        {
            public string Data { get; set; }
            public string Name { get; set; }
            public bool Searchable { get; set; }
            public bool Orderable { get; set; }
            public DtSearch Search { get; set; }
        }

        public class DtOrder
        {
            public int Column { get; set; }
            public DtOrderDir Dir { get; set; }
        }

        public enum DtOrderDir
        {
            Asc,
            Desc
        }

        /// <summary>
        /// A search, as sent by jQuery DataTables when doing AJAX queries.
        /// </summary>
        public class DtSearch
        {
            /// <summary>
            /// Global search value. To be applied to all columns which have searchable as true.
            /// </summary>
            public string Value { get; set; }

            /// <summary>
            /// true if the global filter should be treated as a regular expression for advanced searching, false otherwise.
            /// Note that normally server-side processing scripts will not perform regular expression searching for performance reasons on large data sets, but it is technically possible and at the discretion of your script.
            /// </summary>
            public bool Regex { get; set; }
        }
    }
}