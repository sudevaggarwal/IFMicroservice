using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;

namespace CommonUtilitesForAll
{
    public static class CommonUtilitesClass
    {
        public static string GetKeyValultEndPoint(string vaultName)
        {
            if(!string.IsNullOrEmpty(vaultName))
            {
                return $"https://{vaultName}.vault.azure.net/";
            }
            return string.Empty;
        }

        public static List<List<long>> CrossJoin(List<long[]> arrays)
        {
            var data = arrays.Select(x => x.ToList()).ToList();
            List<List<long>> result = data[0].Select(x => new List<long> { x }).ToList();
            for(var i = 1; i < data.Count; i++)
            {
                result = (from a in result
                          from b in data[i]
                          select new { a, b })
                        .Select(x => x.a.Concat(new List<long> { x.b }).ToList()).ToList();
            }
            return result;
        }

        public static List<dynamic> ToDynamicList(DataTable dt)
        {
            List<dynamic> list = new List<dynamic>();
            foreach (DataRow row in dt.Rows)
            {
                dynamic dyn = new ExpandoObject();
                list.Add(dyn);
                foreach (DataColumn column in dt.Columns)
                {
                    IDictionary<string, object> dic = (IDictionary<string, object>)dyn;
                    dic[column.ColumnName] = row[column];
                }
               
            }
            return list;
        }

        public static string ValidationHelper(object model)
        {
            ValidationContext context = new ValidationContext(model, serviceProvider: null, items: null);
            List<ValidationResult> validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(model, context, validationResults, true);
            string validationResult = isValid ? string.Empty : string.Join("\n", validationResults.Select(x => x.ErrorMessage));
            return validationResult;
        }

        public static bool ExtensionValidator(string[] files,string validExtensionsCommaSaperated)
        {
            for(int index = 0; index < files.Length; index++)
            {
                if (!validExtensionsCommaSaperated.Split(',').Contains(Path.GetExtension(files[index])))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool ExtensionValidator(IFormFileCollection files, string validExtensionsCommaSaperated)
        {
            for (int index = 0; index < files.Count; index++)
            {
                if (!validExtensionsCommaSaperated.Split(',').Contains(Path.GetExtension(files[index]?.FileName?.ToLower())))
                {
                    return true;
                }
            }
            return false;
        }
    }

    public class PrefixKeyVaultSecretManagerUpdated : KeyVaultSecretManager
    {
        private readonly string _prefix;

        public PrefixKeyVaultSecretManagerUpdated(string prefix)
        {
            _prefix = $"{prefix}-";
        }

        public bool Load(Microsoft.Azure.KeyVault.Models.SecretItem secret)
        {
            return secret.Identifier.Name.StartsWith(_prefix);
        }

        public override bool Load(SecretProperties secret)
        {
            return secret.Name.StartsWith(_prefix);
        }

        public override string GetKey(KeyVaultSecret secret)
        {
            return secret.Name.Substring(_prefix.Length).Replace("--", ConfigurationPath.KeyDelimiter);
        }
    }
}
