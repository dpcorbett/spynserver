using SharpTools.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using WPFSpyn.Properties;

namespace WPFSpyn.Model
{
    [Serializable()]
    public class SyncPair : IDataErrorInfo
    {

        #region Properties

        /// <summary>
        /// Gets/Sets the display name of the pair
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets/Sets public status of SyncPair
        /// </summary>
        public bool IsFullSync { get; set; }

        /// <summary>
        /// Gets/Sets the display name of the pair 
        /// 
        /// TODO Define the variable
        /// </summary>
        public string SyncType { get; set; }

        /// <summary>
        /// Gets/sets the root directory of data source.
        /// </summary>
        public string SrcRoot { get; set; }

        /// <summary>
        /// Gets/sets the root directory of data destination.
        /// </summary>
        public string DstRoot { get; set; }

        #endregion


        #region Constructors

        public static SyncPair CreateNewSyncPair()
        {
            return new SyncPair();
        }


        public static SyncPair CreateSyncPair(string p_strDescription, bool p_booIsFullSync, string p_strSyncType, string p_strSrcRoot, string p_strDstRoot)
        {
            return new SyncPair
            {
                Description = p_strDescription,
                IsFullSync = p_booIsFullSync,
                SyncType = p_strSyncType,
                SrcRoot = p_strSrcRoot,
                DstRoot = p_strDstRoot,
            };
        }
        

        protected SyncPair()
        {
        }
              
        #endregion


        #region IDataErrorInfo Members

        string IDataErrorInfo.Error { get { return null; } }

        string IDataErrorInfo.this[string propertyName]
        {
            get { return this.GetValidationError(propertyName); }
        }

        #endregion // IDataErrorInfo Members


        #region Validation

        /// <summary>
        /// Returns true if this object has no validation errors.
        /// </summary>
        public bool IsValid
        {
            get
            {
                foreach (string property in ValidatedProperties)
                    if (GetValidationError(property) != null)
                        return false;

                return true;
            }
        }

        static readonly string[] ValidatedProperties = 
        { 
            // TODO Add SyncType
            "Description",
            //"SyncType",
            "SrcRoot", 
            "DstRoot"
        };

        string GetValidationError(string propertyName)
        {
            if (Array.IndexOf(ValidatedProperties, propertyName) < 0)
                return null;

            string error = null;

            switch (propertyName)
            {
                case "Description":
                    error = this.ValidateDescription();
                    break;

                //case "SyncType":
                //    error = this.ValidateSyncType();
                //    break;

                case "SrcRoot":
                    error = this.ValidateSource();
                    break;

                case "DstRoot":
                    error = this.ValidateDestination();
                    break;

                default:
                    Debug.Fail("Unexpected property being validated on root dir: " + propertyName);
                    break;
            }

            return error;
        }


        string ValidateDescription()
        {
            // TODO Check for existing identical Description
            if (SharpToolsUtility.IsStringEmpty(this.Description))
            {
                return "No valid description";
            }
            return null;
        }


   /**     string ValidateSyncType()
        {
            if (this.SyncType != Strings.SyncPairViewModel_SyncPairTypeOption_FullSync || this.SyncType != Strings.SyncPairViewModel_SyncPairTypeOption_PushSync)
            {
                return "Fuck Off";
            }
            return null;
        }*/
        
        string ValidateSource()
        {
            if (SharpToolsUtility.IsStringEmpty(this.SrcRoot) || !System.IO.Directory.Exists(SrcRoot))
            {
                return "No valid source root";
            }
            else if (!(SharpTools.File.Access.SharpToolsFileAccess.checkDirectoryAuth(this.SrcRoot, WindowsIdentity.GetCurrent().Name)))
            {
                return "No write access for " + WindowsIdentity.GetCurrent().Name;
            }
            return null;
        }


        string ValidateDestination()
        {
            if (SharpToolsUtility.IsStringEmpty(this.DstRoot) || !System.IO.Directory.Exists(this.DstRoot))
            {
                return "No valid destination root";
            }
            else if (!(SharpTools.File.Access.SharpToolsFileAccess.checkDirectoryAuth(this.DstRoot, WindowsIdentity.GetCurrent().Name)))
            {
                return "No write access for " + WindowsIdentity.GetCurrent().Name;
            }
            return null;
        }

        #endregion // Validation


    }
}
