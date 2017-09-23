//using SharpTools.Utility;
using SharpTools.Utility;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;
using WPFSpyn.Library;
using WPFSpyn.Properties;

namespace WPFSpyn.Model
{
    /// <summary>
    /// Stores individual syncpair data.
    /// </summary>
    [Serializable()]
    public class SyncPair : ISyncPair, IDataErrorInfo
    {

        #region Members

        private string m_strDescription;
        private bool m_isFullSync;

        #endregion

        #region Properties

        /// <summary>
        /// Gets/Sets the display name of the pair
        /// </summary>
        public string Description
        {
            get
            {
                return m_strDescription;
            }
            set
            {
                m_strDescription = value;
            }
        }

        /// <summary>
        /// Gets/Sets public status of SyncPair
        /// </summary>
        public bool IsFullSync
        {
            get
            {
                return m_isFullSync;
            }
            set
            {
                m_isFullSync = value;
            }
        }

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

        /// <summary>
        /// Empty constructor
        /// </summary>
        public SyncPair()
        {
        }

        /// <summary>
        /// Static method returns initialised syncpair object.
        /// </summary>
        /// <returns></returns>
        public static SyncPair CreateNewSyncPair()
        {
            return new SyncPair();
        }

        #endregion


        #region IDataErrorInfo Members

        // Error property returns null.
        string IDataErrorInfo.Error { get { return null; } }

        // Return validation error for supplied property name.
        string IDataErrorInfo.this[string propertyName] { get { return GetValidationError(propertyName); } }

        #endregion // IDataErrorInfo Members


        #region Validation

        /// <summary>
        /// Returns true if this object has no validation errors.
        /// </summary>
        public bool IsValid
        {
            get
            {
                // Check each proprty listed for validation errors.
                foreach (string property in ValidatedProperties)
                    // Break and return false if a validation error is found.
                    if (GetValidationError(property) != null)
                        return false;
                // If validation passes return true.
                return true;
            }
        }

        // Store objects to be validated.
        static readonly string[] ValidatedProperties = 
        { 
            "Description",
            "SyncType",
            "SrcRoot", 
            "DstRoot"
        };

        /// <summary>
        /// Call validation on property specified and return null if successful.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        string GetValidationError(string propertyName)
        {
            // Check if proprety name supplid is listed in validated properties.
            if (Array.IndexOf(ValidatedProperties, propertyName) < 0)
                return null;
            // Default response to null.
            string error = null;
            // Select method via proprty name.
            switch (propertyName)
            {
                case "Description":
                    error = ValidateDescription();
                    break;

                case "SyncType":
                    error = ValidateSyncType();
                    break;

                case "SrcRoot":
                    error = ValidateSource();
                    break;

                case "DstRoot":
                    error = ValidateDestination();
                    break;

                default:
                    Debug.Fail("Unexpected property being validated on root dir: " + propertyName);
                    break;
            }
            // Respond with any validation errors, or null.
            return error;
        }

        /// <summary>
        /// Check that description has content.
        /// </summary>
        /// <returns></returns>
        string ValidateDescription()
        {
            var stu = new SharpToolsUtility();
            // TODO Check for existing identical Description
            if (stu.IsStringEmpty(Description))
            {
                return "No valid description";
            }
            return null;
        }

        /// <summary>
        /// Check that a sync type has been selected.
        /// </summary>
        /// <returns></returns>
        string ValidateSyncType()
        {
            if (SyncType != Strings.SyncPairViewModel_SyncPairTypeOption_FullSync && SyncType != Strings.SyncPairViewModel_SyncPairTypeOption_PushSync)
            {
                return "No Valid sync type";
            }
            return null;
        }
        
        /// <summary>
        /// Check that source path is valid and user has access.
        /// </summary>
        /// <returns></returns>
        string ValidateSource()
        {
            var stu = new SharpToolsUtility();
            if (stu.IsStringEmpty(SrcRoot) || !System.IO.Directory.Exists(SrcRoot))
            {
                return "No valid source root";
            }
            // TODO Account for network seurity.
            else if (!(SharpTools.File.Access.SharpToolsFileAccess.DirectoryHasPermission(SrcRoot, System.Security.AccessControl.FileSystemRights.Write)))
            {
                return "No write access for " + WindowsIdentity.GetCurrent().Name;
            }
            return null;
        }

        /// <summary>
        /// Check that destination path is valid and user has access.
        /// </summary>
        /// <returns></returns>
        string ValidateDestination()
        {
            var stu = new SharpToolsUtility();
            if (stu.IsStringEmpty(DstRoot) || !System.IO.Directory.Exists(DstRoot))
            {
                return "No valid destination root";
            }
            // TODO Account for network seurity.
            else if (!(SharpTools.File.Access.SharpToolsFileAccess.DirectoryHasPermission(DstRoot, System.Security.AccessControl.FileSystemRights.Write)))
            {
                return "No write access for " + WindowsIdentity.GetCurrent().Name;
            }
            return null;
        }

        #endregion // Validation

    }
}
