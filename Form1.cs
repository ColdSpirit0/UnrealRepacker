using System.Diagnostics;
using System.Text.RegularExpressions;
using UAssetAPI;
using UAssetAPI.UnrealTypes;

namespace UnrealRepacker;

public partial class Form1 : Form
{
    Config config;
    // PakTool pakTool;
    Repacker repacker;

    public Form1()
    {
        InitializeComponent();
        config = new Config(Directory.GetCurrentDirectory());
        // pakTool = new PakTool(config);
        repacker = new Repacker(config);
    }

    private void Form1_Shown(object sender, EventArgs e)
    {
        // Load config
        string configFile = "config.cfg";
        if (File.Exists(configFile))
        {
            config.Load(configFile);
        }
        else
        {
            MessageBox.Show("config.cfg not found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }
    }

    private void OnAddPakButtonClick(object sender, EventArgs e)
    {
        OpenFileDialog ofd = new OpenFileDialog
        {
            Filter = "Pak files (*.pak)|*.pak"
        };

        if (ofd.ShowDialog() == DialogResult.OK)
        {
            var paks = repacker.SearchPaksForSameMod(ofd.FileName);
            foreach (var pak in paks)
            {
                repacker.AddPak(pak);
            }
            UpdatePaksList();
            UpdateModData();
        }
    }

    private void UpdateModData()
    {
        comboBox1.Items.Clear();
        foreach (var mod in repacker.Mods)
        {
            comboBox1.Items.Add(mod);
        }

        comboBox1.SelectedItem = repacker.MainMod;
        newModNameTextBox.Text = repacker.MainMod!.modName;
    }

    private void UpdatePaksList()
    {
        treeView1.Nodes.Clear();

        foreach (var mod in repacker.Mods)
        {
            TreeNode modNode = new TreeNode(mod.modName);
            foreach (var pak in mod.paks)
            {
                modNode.Nodes.Add(new TreeNode($"{pak.PakName}, {pak.NetworkType}, {pak.PakOsType}"));
            }
            treeView1.Nodes.Add(modNode);
        }

        treeView1.ExpandAll();
    }

    private void OnPackButtonClick(object sender, EventArgs e)
    {
        // validate uassets are selected
        if (uassetsListBox.SelectedItems.Count == 0)
        {
            MessageBox.Show("No uassets are selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        repacker.Pack(uassetsListBox.SelectedItems.Cast<string>(), newModNameTextBox.Text);

        // foreach (var modInfo in modInfos)
        // {
        //     pakTool.PackUassetFiles(uassetsListBox.SelectedItems.Cast<string>(), modInfo, removeMetadataCheckBox.Checked);
        // }
    }

    private void OnExtractPaksButtonClick(object sender, EventArgs e)
    {
        repacker.ExtractAllPaks();
        foreach (var import in repacker.GetMainModImports())
        {
            uassetsListBox.Items.Add(import);
        }
    }
}






