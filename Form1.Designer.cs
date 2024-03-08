namespace UnrealRepacker
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            button1 = new Button();
            label1 = new Label();
            label2 = new Label();
            uassetsListBox = new ListBox();
            packButton = new Button();
            treeView1 = new TreeView();
            extractPaksButton = new Button();
            tabControl1 = new TabControl();
            addPaksTab = new TabPage();
            comboBox1 = new ComboBox();
            label4 = new Label();
            assetsTab = new TabPage();
            newModNameTextBox = new TextBox();
            label3 = new Label();
            tabControl1.SuspendLayout();
            addPaksTab.SuspendLayout();
            assetsTab.SuspendLayout();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button1.Location = new Point(845, 21);
            button1.Name = "button1";
            button1.Size = new Size(186, 23);
            button1.TabIndex = 0;
            button1.Text = "Add paks";
            button1.UseVisualStyleBackColor = true;
            button1.Click += OnAddPakButtonClick;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 3);
            label1.Name = "label1";
            label1.Size = new Size(68, 15);
            label1.TabIndex = 2;
            label1.Text = "Found paks";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(6, 3);
            label2.Name = "label2";
            label2.Size = new Size(314, 15);
            label2.TabIndex = 3;
            label2.Text = "Select files to pack, use ctrl or shift to select multiple items";
            // 
            // uassetsListBox
            // 
            uassetsListBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            uassetsListBox.FormattingEnabled = true;
            uassetsListBox.ItemHeight = 15;
            uassetsListBox.Location = new Point(6, 21);
            uassetsListBox.Name = "uassetsListBox";
            uassetsListBox.SelectionMode = SelectionMode.MultiExtended;
            uassetsListBox.Size = new Size(837, 484);
            uassetsListBox.TabIndex = 6;
            // 
            // packButton
            // 
            packButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            packButton.Location = new Point(849, 482);
            packButton.Name = "packButton";
            packButton.Size = new Size(182, 23);
            packButton.TabIndex = 7;
            packButton.Text = "Pack";
            packButton.UseVisualStyleBackColor = true;
            packButton.Click += OnPackButtonClick;
            // 
            // treeView1
            // 
            treeView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            treeView1.Location = new Point(6, 21);
            treeView1.Name = "treeView1";
            treeView1.Size = new Size(833, 485);
            treeView1.TabIndex = 9;
            // 
            // extractPaksButton
            // 
            extractPaksButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            extractPaksButton.Location = new Point(849, 21);
            extractPaksButton.Name = "extractPaksButton";
            extractPaksButton.Size = new Size(182, 23);
            extractPaksButton.TabIndex = 10;
            extractPaksButton.Text = "Extract Paks";
            extractPaksButton.UseVisualStyleBackColor = true;
            extractPaksButton.Click += OnExtractPaksButtonClick;
            // 
            // tabControl1
            // 
            tabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl1.Controls.Add(addPaksTab);
            tabControl1.Controls.Add(assetsTab);
            tabControl1.Location = new Point(12, 12);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1045, 540);
            tabControl1.TabIndex = 11;
            // 
            // addPaksTab
            // 
            addPaksTab.Controls.Add(comboBox1);
            addPaksTab.Controls.Add(label4);
            addPaksTab.Controls.Add(label1);
            addPaksTab.Controls.Add(treeView1);
            addPaksTab.Controls.Add(button1);
            addPaksTab.Location = new Point(4, 24);
            addPaksTab.Name = "addPaksTab";
            addPaksTab.Padding = new Padding(3);
            addPaksTab.Size = new Size(1037, 512);
            addPaksTab.TabIndex = 0;
            addPaksTab.Text = "Add Paks";
            addPaksTab.UseVisualStyleBackColor = true;
            // 
            // comboBox1
            // 
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(845, 88);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(186, 23);
            comboBox1.TabIndex = 11;
            comboBox1.SelectedIndexChanged += OnMainModSelecedItemhanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(845, 70);
            label4.Name = "label4";
            label4.Size = new Size(62, 15);
            label4.TabIndex = 10;
            label4.Text = "Main mod";
            // 
            // assetsTab
            // 
            assetsTab.Controls.Add(newModNameTextBox);
            assetsTab.Controls.Add(label3);
            assetsTab.Controls.Add(label2);
            assetsTab.Controls.Add(extractPaksButton);
            assetsTab.Controls.Add(uassetsListBox);
            assetsTab.Controls.Add(packButton);
            assetsTab.Location = new Point(4, 24);
            assetsTab.Name = "assetsTab";
            assetsTab.Padding = new Padding(3);
            assetsTab.Size = new Size(1037, 512);
            assetsTab.TabIndex = 1;
            assetsTab.Text = "Repack";
            assetsTab.UseVisualStyleBackColor = true;
            // 
            // newModNameTextBox
            // 
            newModNameTextBox.Location = new Point(849, 453);
            newModNameTextBox.Name = "newModNameTextBox";
            newModNameTextBox.Size = new Size(182, 23);
            newModNameTextBox.TabIndex = 12;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(849, 435);
            label3.Name = "label3";
            label3.Size = new Size(92, 15);
            label3.TabIndex = 11;
            label3.Text = "New mod name";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1069, 564);
            Controls.Add(tabControl1);
            Name = "Form1";
            Text = "Form1";
            Shown += Form1_Shown;
            tabControl1.ResumeLayout(false);
            addPaksTab.ResumeLayout(false);
            addPaksTab.PerformLayout();
            assetsTab.ResumeLayout(false);
            assetsTab.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Button button1;
        private Label label1;
        private Label label2;
        private Label label3;
        private TextBox newModNameTextBox;
        private ListBox uassetsListBox;
        private Button packButton;
        private TreeView treeView1;
        private Button extractPaksButton;
        private TabControl tabControl1;
        private TabPage addPaksTab;
        private TabPage tabPage1;
        private TabPage assetsTab;
        private ComboBox comboBox1;
        private Label label4;
    }
}
