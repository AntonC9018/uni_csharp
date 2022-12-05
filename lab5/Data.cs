using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace lab5;

public class Person
{
    public PersonName Name { get; set; } = new();
    public PersonLocation Location { get; set; } = new();
    public PersonStudy Study { get; set; } = new();

    public static readonly PropertyInfo[] Categories = typeof(Person).GetProperties();
    public static readonly PropertyInfo[][] Properties = Categories
        .Select(x => x.PropertyType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        .ToArray();
}

public class PersonName
{
    public string? Name { get; set; }
    public string? LastName { get; set; }
}

public class PersonLocation
{
    public string? Address { get; set; }
    public string? City { get; set; }
}

public class PersonStudy
{
    public string? University { get; set; }
    public string? Faculty { get; set; }
}

public class PersonIdentification
{
    public string? Name;
}


public class AddPersonForm : Form
{
    private readonly Person _person;
    private readonly List<TextBox> _inputBoxes;
    private readonly Button _finishButton;

    public AddPersonForm(Person person)
    {
        _person = person;
        _inputBoxes = new();
        _finishButton = new Button();
        var layout = new FlowLayoutPanel();
        layout.Size = Size;
        layout.FlowDirection = FlowDirection.TopDown;
        layout.WrapContents = true;

        for (int i = 0; i < Person.Categories.Length; i++)
        {
            var categoryProp = Person.Categories[i];
            var groupBox = new GroupBox();
            groupBox.Text = categoryProp.Name;
            groupBox.AutoSize = true;
            groupBox.Margin = new Padding(5);

            var layout1 = new FlowLayoutPanel();
            layout1.FlowDirection = FlowDirection.TopDown;
            layout1.Dock = DockStyle.Fill;
            layout1.AutoSize = true;

            foreach (var prop in Person.Properties[i])
            {
                var label = new Label();
                label.Text = prop.Name;

                var inputBox = new TextBox();
                int index = _inputBoxes.Count;
                inputBox.TextChanged += (sender, _) => 
                {
                    var value = ((TextBox) sender!).Text;

                    if (string.IsNullOrEmpty(value))
                        _finishButton.Enabled = false;
                    else
                        _finishButton.Enabled = _inputBoxes.All(box => !string.IsNullOrEmpty(box.Text));
                    
                    var category = categoryProp.GetValue(person);
                    prop.SetValue(category, value);
                };
                _inputBoxes.Add(inputBox);

                layout1.Controls.Add(label);
                layout1.Controls.Add(inputBox);
            }

            groupBox.Controls.Add(layout1);
            layout.Controls.Add(groupBox);
        }

        _finishButton.Enabled = false;
        _finishButton.Text = "Finish";
        _finishButton.Click += (_, _) =>
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        };

        layout.Controls.Add(_finishButton);
        this.Controls.Add(layout);
    }
}

public class RemovePersonForm : Form
{
    public RemovePersonForm(PersonIdentification person)
    {
        var layout = new FlowLayoutPanel();
        var label = new Label();
        var inputBox = new TextBox();
        var button = new Button();

        inputBox.TextChanged += (sender, _) =>
        {
            var value = ((TextBox) sender!).Text;
            button.Enabled = !string.IsNullOrEmpty(value);
            person.Name = value;
        };

        label.Text = "Name";
        button.Enabled = false;
        button.Text = "Remove";
        button.Click += (_, _) =>
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        };

        layout.Controls.Add(label);
        layout.Controls.Add(inputBox);
        layout.Controls.Add(button);

        this.Controls.Add(layout);
    }
}

public class EditPersonForm : Form
{
    private readonly List<TextBox> _inputBoxes;

    public EditPersonForm(object category)
    {
        _inputBoxes = new();

        var layout = new FlowLayoutPanel();
        layout.FlowDirection = FlowDirection.TopDown;
        layout.Dock = DockStyle.Fill;
        layout.WrapContents = true;
        layout.Size = Size;

        var finishButton = new Button();
        finishButton.Enabled = false;
        finishButton.Text = "Finish";
        finishButton.Click += (_, _) =>
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        };

        var groupBox = new GroupBox();
        groupBox.Text = category.GetType().Name;
        groupBox.AutoSize = true;

        var layout1 = new FlowLayoutPanel();
        layout1.FlowDirection = FlowDirection.TopDown;
        layout1.Dock = DockStyle.Fill;
        layout1.AutoSize = true;
        layout1.WrapContents = false;

        groupBox.Controls.Add(layout1);

        foreach (var prop in category.GetType().GetProperties())
        {
            var label = new Label();
            label.Text = prop.Name;

            var inputBox = new TextBox();
            inputBox.TextChanged += (sender, _) => 
            {
                var value = ((TextBox) sender!).Text;

                if (string.IsNullOrEmpty(value))
                    finishButton.Enabled = false;
                else
                    finishButton.Enabled = _inputBoxes.All(box => !string.IsNullOrEmpty(box.Text));
                    
                prop.SetValue(category, value);
            };
            _inputBoxes.Add(inputBox);

            layout1.Controls.Add(label);
            layout1.Controls.Add(inputBox);
        }

        layout.Controls.Add(groupBox);
        layout.Controls.Add(finishButton);
        this.Controls.Add(layout);
    }
}