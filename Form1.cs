using System.Diagnostics;
using System.Text.RegularExpressions;
using UAssetAPI;
using UAssetAPI.UnrealTypes;

namespace UnrealRepacker;

public partial class Form1 : Form
{
    Config config;
    // PakTool pakTool;
    IRepacker repacker;

    List<ModInfo> modInfos = new();

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
        }
    }

    private void UpdatePaksList()
    {
        var modGroups = repacker.Paks.GroupBy(x => x.modName);
        treeView1.Nodes.Clear();

        foreach (var modGroup in modGroups)
        {
            TreeNode modNode = new TreeNode(modGroup.Key);
            foreach (var pak in modGroup)
            {
                modNode.Nodes.Add(new TreeNode($"{pak.pakName}, {pak.networkType}, {pak.pakOsType}"));
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

        repacker.Pack(uassetsListBox.SelectedItems.Cast<string>());

        // foreach (var modInfo in modInfos)
        // {
        //     pakTool.PackUassetFiles(uassetsListBox.SelectedItems.Cast<string>(), modInfo, removeMetadataCheckBox.Checked);
        // }
    }

    private void OnExtractPaksButtonClick(object sender, EventArgs e)
    {
        repacker.ExtractAllPaks();
        foreach (var import in repacker.GetImports())
        {
            uassetsListBox.Items.Add(import);
        }
    }
}






