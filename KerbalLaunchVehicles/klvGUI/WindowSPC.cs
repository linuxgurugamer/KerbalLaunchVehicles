using KSP.UI.Screens;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using static KerbalLaunchVehicles.SaveManager;

namespace KerbalLaunchVehicles.klvGUI
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    internal class WindowSPC : WindowDefault
    {
        public override void Awake()
        {
            base.Awake();
            //visibleInScenes = ApplicationLauncher.AppScenes.SPACECENTER;
            WindowRect = new Rect(GlobalSettings.SPCWindowPos, new Vector2(450, 375));
            KLVCore.SPCWindow = this;
        }

        public override void Start()
        {
            base.Start();
            openTab = ViewTab.Families;
            SaveManager.RefreshPaths();
        }

        // Destinations Tab
        private GUITextBox textAddDestination;
        private GUIButton buttonSelectDefaultDest;
        private GUIButton buttonSetDestinations;
        private GUIButton buttonAddDest;
        private GUIButton buttonRemoveDest;
        private GUIButton buttonRemoveAllDest;

        bool selectDefaultDest = false;

        // Settings Tab
        private GUITextBox textFolderPath;
        private GUIButton buttonSave;
        private GUIButton buttonLoad;
        private GUIButton buttonUnload;
        private GUIButton buttonIncreaseFont;
        private GUIButton buttonDecreaseFont;

        protected override void CreateControls()
        {
            base.CreateControls();

            // Destinations Tab
            textAddDestination = new GUITextBox("Add Destination: ", "", "", 180, 300, DoAddDestination);
            buttonSelectDefaultDest = new GUIButton("Select Default", DoLoadDefaults, new GUILayoutOption[] { GUILayout.Width(120) });
            buttonSetDestinations = new GUIButton("Select", DoSelectDefaultConfig, new GUILayoutOption[] { GUILayout.Width(120) });
            buttonAddDest = new GUIButton("Add", DoAddDestination, new GUILayoutOption[] { GUILayout.Width(120) });
            buttonRemoveDest = new GUIButton("Remove", DoRemoveDestination, new GUILayoutOption[] { GUILayout.Width(100) });
            buttonRemoveAllDest = new GUIButton("Remove All", DoRemoveAllDestination, new GUILayoutOption[] { GUILayout.Width(100) });

            comboDestination = new DropDown(new Vector2(260, 170), KLVCore.GetAllDestinationName());
            RegisterCombos(comboDestination);

            // Settings Tab
            textFolderPath = new GUITextBox("Config path:", SaveManager.vehiclesPath, "", 300, 400);
            buttonSave = new GUIButton("Save Configurations", DoSave, null);
            buttonLoad = new GUIButton("Load Configurations", DoLoad, null);
            buttonUnload = new GUIButton("Unload All Configurations", DoUnload, null);
            buttonIncreaseFont = new GUIButton("▲", DoIncreaseFont, GUILayout.Width(30));
            buttonDecreaseFont = new GUIButton("▼", DoDecreaseFont, GUILayout.Width(30));
        }

        public override void OnWindow(int windowId)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            buttonTabFamilies.DoLayout(openTab == ViewTab.Families ? klvGUIStyles.TabButtonActive : klvGUIStyles.TabButton, null, ViewTab.Families);
            buttonTabDestinations.DoLayout(openTab == ViewTab.Destinations ? klvGUIStyles.TabButtonActive : klvGUIStyles.TabButton, null, ViewTab.Destinations);
            buttonTabSettings.DoLayout(openTab == ViewTab.Settings ? klvGUIStyles.TabButtonActive : klvGUIStyles.TabButton, null, ViewTab.Settings);
            GUILayout.EndHorizontal();

            tabDivider.DoLayout();

            switch (openTab)
            {
                case ViewTab.Families: { DisplayFamilies(); break; }
                case ViewTab.Destinations: { DisplayDestinations(); break; }
                case ViewTab.Settings: { DisplaySettings(); break; }
                default: { break; }
            }

            GUILayout.EndVertical();
            GUI.DragWindow();
            GlobalSettings.SPCWindowPos = WindowRect.position;
        }

        //Displays

        protected override void DisplayFamilies(bool forceNoEdit = false)
        {
            LayoutGUIList(familyList, 425, 270);
            base.DisplayFamilies(forceNoEdit);
        }

        private void DisplayDestinations()
        {
            if (string.IsNullOrEmpty(textAddDestination.GetText()))
            {
                LayoutTextInput(textAddDestination, buttonSelectDefaultDest, KLVCore.DestinationAvailable, "Select Default", showAltButtonIfEmpty: true);
            }
            else
            {
                if (selectDefaultDest)
                {
                    MarkComboToExpand(false, comboDestination);
                    comboDestination.SetItems(KLVCore.GetAllDestinationName());
                    selectDefaultDest = false;
                }
                LayoutTextInput(textAddDestination, buttonAddDest, KLVCore.DestinationAvailable, "Already Exists!");
            }
            using (new GUILayout.HorizontalScope())
            {
                if (selectDefaultDest)
                    GUILayout.Label("Default Destination Configs:", klvGUIStyles.StandardLabel);
                else
                    GUILayout.Label("Destinations:", klvGUIStyles.StandardLabel);
                GUILayout.FlexibleSpace();
                buttonRemoveAllDest.DoLayout(klvGUIStyles.StandardButton, "Remove All");
            }
            GUILayout.BeginHorizontal();

            comboDestination.DoLayout(null);

            if (comboDestination.isItemSelected)
            {
                if (selectDefaultDest)
                {
                    if (string.IsNullOrEmpty(textAddDestination.GetText()))
                    {
                        buttonSetDestinations.DoLayout(klvGUIStyles.StandardButton, "Select", comboDestination.selectedItemName);
                    }
                }
                else
                {
                    buttonRemoveDest.DoLayout(klvGUIStyles.StandardButton, "Remove", comboDestination.selectedItemName);
                }
            }

            GUILayout.EndHorizontal();
        }

        private void DisplaySettings()
        {
            textFolderPath.DoLayout(klvGUIStyles.StandardLabel);
            buttonSave.DoLayout(klvGUIStyles.StandardButton);
            buttonLoad.DoLayout(klvGUIStyles.StandardButton);

#if DEBUG
            buttonUnload.DoLayout(klvGUIStyles.StandardButton);
#endif
            GUILayout.Space(5);
            GlobalSettings.AllowVehicleDrop = GUILayout.Toggle(GlobalSettings.AllowVehicleDrop, "  Enable launch vehicle drop zone");
            GUILayout.Space(10);
            GUILayout.BeginHorizontal(GUILayout.Width(270));
            GUILayout.Label("Adjust font size", klvGUIStyles.StandardLabel);
            buttonIncreaseFont.DoLayout(klvGUIStyles.fontSizeRelative < klvGUIStyles.fontSizeMax ? klvGUIStyles.StandardButton : klvGUIStyles.DisabledButton);
            buttonDecreaseFont.DoLayout(klvGUIStyles.fontSizeRelative > klvGUIStyles.fontSizeMin ? klvGUIStyles.StandardButton : klvGUIStyles.DisabledButton);
            GUILayout.EndHorizontal();

            if (klvGUIStyles.fontSizeRelative != 0)
            {
                GUILayout.Label("Not default size - some elements may not display correctly", klvGUIStyles.WarningLabel);
            }
            else
            {
                GUILayout.Label("Default size", klvGUIStyles.StandardLabel);
            }
        }

        //Button Actions
        protected override void DoChangeTab(GUIButton sender, object value)
        {
            comboDestination.SetExpanded(false);
            base.DoChangeTab(sender, value);
        }

        //Destinations

        private void DoRemoveDestination(GUIButton sender, object value)
        {
            if (!string.IsNullOrEmpty(value.ToString()))
            {
                KLVCore.RemoveDestination(value.ToString());
                KLVCore.Save();
                KLVCore.UpdateAllVehicleNameSchemes();
                comboDestination.SetItems(KLVCore.GetAllDestinationName());
                comboDestination.Deselect();
            }
        }

        private void DoRemoveAllDestination(GUIButton sender, object value)
        {
                KLVCore.AllDestinations.Clear();
                KLVCore.Save();
                KLVCore.UpdateAllVehicleNameSchemes();
                comboDestination.SetItems(KLVCore.GetAllDestinationName());
                comboDestination.Deselect();
        }

        private void DoAddDestination(GUIButton sender, object value)
        {
            if (!string.IsNullOrEmpty(value.ToString()) && KLVCore.DestinationAvailable(value.ToString()))
            {
                textAddDestination.SetEditing(false);
                textAddDestination.SetText("");
                KLVCore.AddDestination(value.ToString());
                KLVCore.Save();
                KLVCore.UpdateAllVehicleNameSchemes();
                comboDestination.SetItems(KLVCore.GetAllDestinationName());
                //Open combo to view new addition
                MarkComboToExpand(true, comboDestination);
            }
        }

        //Settings
        private void DoSave(GUIButton sender, object value)
        {
            KLVCore.Save();
        }

        private void DoLoad(GUIButton sender, object value)
        {
            KLVCore.Load();
            UpdateFamilies();
            comboDestination.SetItems(KLVCore.GetAllDestinationName());
        }
        private void DoUnload(GUIButton sender, object value)
        {
            KLVCore.RefreshAllConfigurations();
            UpdateFamilies();
            comboDestination.SetItems(KLVCore.GetAllDestinationName());
        }

        #region SelectDefaultConfig
        internal static List<string> GetAllDefaultDestinationNames(List<Destinations> destinations)
        {
            var _allDestinationNames = destinations.Select(x => x.descr).ToList();
            return _allDestinationNames;
        }

        private void DoLoadDefaults(GUIButton sender, object value)
        {
            KLVCore.Load();
            List<Destinations> dests = SaveManager.GetDefaultDestinations();
            comboDestination.SetItems(GetAllDefaultDestinationNames(dests));
            //Open combo to view new addition
            MarkComboToExpand(true, comboDestination);
            selectDefaultDest = true;
        }

        private void DoSelectDefaultConfig(GUIButton sender, object value)
        {
            List<Destinations> dests = SaveManager.GetDefaultDestinations();
            comboDestination.SetItems(GetAllDefaultDestinationNames(dests));

            foreach (var d in dests)
            {
                if (d.descr == value.ToString())
                {
                    foreach (var dest in d.destinationList)
                    {
                        KLVCore.AddDestination(dest.Name);
                    }
                }
            }
            KLVCore.Save();
            KLVCore.UpdateAllVehicleNameSchemes();

            comboDestination.Deselect();
            selectDefaultDest = false;
            comboDestination.SetItems(KLVCore.GetAllDestinationName());

        }

        #endregion

        private void DoIncreaseFont(GUIButton sender, object value)
        {
            klvGUIStyles.AdjustFontSize(1);
        }

        private void DoDecreaseFont(GUIButton sender, object value)
        {
            klvGUIStyles.AdjustFontSize(-1);
        }
    }
}

