﻿//------------------------------------------------------------------------------
// <auto-generated>
//     このコードはツールによって生成されました。
//     ランタイム バージョン:4.0.30319.42000
//
//     このファイルへの変更は、以下の状況下で不正な動作の原因になったり、
//     コードが再生成されるときに損失したりします。
// </auto-generated>
//------------------------------------------------------------------------------

namespace DecisionTableMakerApp.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.13.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string LastFactorLevelText {
            get {
                return ((string)(this["LastFactorLevelText"]));
            }
            set {
                this["LastFactorLevelText"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("[FactorA(LevelA1,LevelA2,LevelA3)] * [FactorB(LevelB1,LevelB2)] + [FactorC(LevelC" +
            "1,LevelC2)]")]
        public string LastFormulaText {
            get {
                return ((string)(this["LastFormulaText"]));
            }
            set {
                this["LastFormulaText"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("結果|実施日||実施者||結果")]
        public string LastAdditionalSettings {
            get {
                return ((string)(this["LastAdditionalSettings"]));
            }
            set {
                this["LastAdditionalSettings"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool LastIsIgnoreWhiteSpace {
            get {
                return ((bool)(this["LastIsIgnoreWhiteSpace"]));
            }
            set {
                this["LastIsIgnoreWhiteSpace"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("未設定")]
        public string LastAuthor {
            get {
                return ((string)(this["LastAuthor"]));
            }
            set {
                this["LastAuthor"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("未設定")]
        public string LastInspection {
            get {
                return ((string)(this["LastInspection"]));
            }
            set {
                this["LastInspection"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("50")]
        public string RandomSearchNum {
            get {
                return ((string)(this["RandomSearchNum"]));
            }
            set {
                this["RandomSearchNum"] = value;
            }
        }
    }
}
