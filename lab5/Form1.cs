using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace lab5;

public partial class Form1 : Form
{
    private readonly List<ListBox>[] _listBoxes;
    private int _selectedPersonIndex = -1;
    private int SelectedPersonIndex
    {
        get => _selectedPersonIndex;
        set
        {
            if (_selectedPersonIndex == value)
                return;

            _selectedPersonIndex = value;

            bool shouldEnable = ShouldEnableEditingOrRemoval();
            this.editToolStripMenuItem.Enabled = shouldEnable;
        }
    }

    private int PersonsCount => _listBoxes[0][0].Items.Count;

    private bool ShouldEnableEditingOrRemoval()
    {
        return _listBoxes[0][0].Items.Count > 0 && _selectedPersonIndex >= 0;
    }

    private TabPage GetPage(int index) => this.tabControl1.TabPages[index];


    public Form1()
    {
        InitializeComponent();

        _listBoxes = new List<ListBox>[Person.Categories.Length];

        this.addToolStripMenuItem.Click += (_, _) =>
        {
            var person = new Person();
            var dialog = new AddPersonForm(person);
            var result = dialog.ShowDialog();
            if (result != DialogResult.OK)
                return;

            for (int i = 0; i < Person.Categories.Length; i++)
            {
                var categoryProp = Person.Categories[i];
                var value = categoryProp.GetValue(person);
                var properties = Person.Properties[i];

                for (int j = 0; j < properties.Length; j++)
                {
                    var property = properties[j];
                    var propertyValue = property.GetValue(value);
                    var listBox = _listBoxes[i][j];
                    listBox.Items.Add(propertyValue!);
                }
            }

            SelectedPersonIndex = PersonsCount - 1;
            this.deleteToolStripMenuItem.Enabled = true;
        };

        this.editToolStripMenuItem.Enabled = false;
        this.editToolStripMenuItem.Click += (_, _) =>
        {
            int selectedPropertyIndex = this.tabControl1.SelectedIndex;
            var categoryProperty = Person.Categories[selectedPropertyIndex];
            var categoryObject = Activator.CreateInstance(categoryProperty.PropertyType);
            int selectedPersonIndex = _selectedPersonIndex;

            var dialog = new EditPersonForm(categoryObject!);
            var result = dialog.ShowDialog();
            if (result != DialogResult.OK)
                return;

            var properties = Person.Properties[selectedPropertyIndex];
            for (int i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                var propertyValue = property.GetValue(categoryObject);
                var listBox = _listBoxes[selectedPropertyIndex][i];
                listBox.Items[selectedPersonIndex] = propertyValue!;
            }
        };

        this.deleteToolStripMenuItem.Enabled = false;
        this.deleteToolStripMenuItem.Click += (_, _) =>
        {
            var person = new PersonIdentification();
            var dialog = new RemovePersonForm(person);
            var result = dialog.ShowDialog();
            if (result != DialogResult.OK)
                return;

            int nameCompoundPropertyIndex = Array.IndexOf(
                Person.Categories,
                typeof(Person).GetProperty(nameof(Person.Name)));
            int namePropertyIndex = Array.IndexOf(
                Person.Properties[nameCompoundPropertyIndex],
                typeof(PersonName).GetProperty(nameof(PersonName.Name))); 
            var nameListBox = _listBoxes[nameCompoundPropertyIndex][namePropertyIndex];
            var nameIndex = nameListBox.Items.IndexOf(person.Name!);
            if (nameIndex == -1)
            {
                MessageBox.Show($"Person {person.Name} not found.");
                return;
            }

            foreach (var listBox in _listBoxes.SelectMany(x => x))
                listBox.Items.RemoveAt(nameIndex);

            SelectedPersonIndex = -1;
            this.deleteToolStripMenuItem.Enabled = PersonsCount == 0;
        };
        
        for (int i = 0; i < Person.Categories.Length; i++)
        {
            var categoryProp = Person.Categories[i];

            var layout = new FlowLayoutPanel();
            layout.FlowDirection = FlowDirection.TopDown;
            layout.Dock = DockStyle.Fill;
            layout.WrapContents = false;

            _listBoxes[i] = new List<ListBox>();

            foreach (var prop in Person.Properties[i])
            {
                var label = new Label();
                label.Text = prop.Name;
                
                var listBox = new ListBox();
                listBox.Dock = DockStyle.Fill;
                listBox.SelectedIndexChanged += (_, _) =>
                {
                    SelectedPersonIndex = listBox.SelectedIndex;
                };
                _listBoxes[i].Add(listBox);

                layout.Controls.Add(label);
                layout.Controls.Add(listBox);
            }

            GetPage(i).Controls.Add(layout);
        }
    }
}