using System.Diagnostics;
using System.Text.RegularExpressions;
using UAssetAPI;
using UAssetAPI.UnrealTypes;

namespace UnrealRepacker;

public partial class Form1 : Form
{
    Config config;
    Repacker repacker;


    public Form1()
    {
        InitializeComponent();
        config = new Config(Directory.GetCurrentDirectory());
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
            return;
        }

        // validate unreal pak tool is exists
        if (!File.Exists(config.UnrealPak))
        {
            MessageBox.Show("UnrealPak.exe not found, check path in config", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
            return;
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

        comboBox1.SelectedItem ??= repacker.MainMod;
    }

    private void UpdatePaksList()
    {
        treeView1.Nodes.Clear();

        foreach (var mod in repacker.Mods)
        {
            TreeNode modNode = new TreeNode(mod.modName);
            foreach (var pak in mod.paks)
            {
                modNode.Nodes.Add(new TreeNode($"{pak.PakName}, {pak.PakType}"));
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
            MessageBox.Show("No uassets are selected, select uassets you want to pack.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // validate mod name is not empty
        if (string.IsNullOrEmpty(newModNameTextBox.Text))
        {
            MessageBox.Show("Mod name is empty, provide a valid name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        repacker.Pack(uassetsListBox.SelectedItems.Cast<string>(), newModNameTextBox.Text);

        // open directory with packed files
        if (openDirectoryCheckBox.Checked)
        {
            Process.Start("explorer.exe", Path.Combine(config.PackedDirectory, newModNameTextBox.Text));
        }
    }

    private void OnExtractPaksButtonClick(object sender, EventArgs e)
    {
        // validate paks are selected
        if (!repacker.Paks.Any())
        {
            MessageBox.Show("No paks were added", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // extract and add imports to listbox
        repacker.ExtractAllPaks();
        foreach (var import in repacker.GetMainModImports())
        {
            uassetsListBox.Items.Add(import);
        }
    }

    private void OnMainModSelecedItemhanged(object sender, EventArgs e)
    {
        repacker.MainMod = (ModInfo)comboBox1.SelectedItem;
        newModNameTextBox.Text = repacker.MainMod.modName;

        // highlight selected mod
        foreach (TreeNode modNode in treeView1.Nodes)
        {
            modNode.BackColor = modNode.Text == repacker.MainMod.modName
                                ? Color.LightBlue
                                : treeView1.BackColor;
        }
    }

    private void OnResetChangesClick(object sender, EventArgs e)
    {
        repacker.Clean();
        uassetsListBox.Items.Clear();
        treeView1.Nodes.Clear();
        newModNameTextBox.Text = "";
        comboBox1.Items.Clear();
    }
}






