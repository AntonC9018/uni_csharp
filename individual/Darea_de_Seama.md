# Tehnologii avansate de programare

Tema: *Atribute în C#.*

Student: *Curmanschii Anton, MIA2022.*


## Introducere

Atributele în C# reprezintă capacitatea limbajului care permite asocia date adăugătoare cu entitățile din cod, ca câmpuri, proprietăți, tipuri, metode, etc.
Atributele permit moduri noi de realizare a sarcinilor în program, realizând atribuirea datelor statice într-un mod declarativ.
Ele sunt des folosite în librării care vor să ofere un API declarativ, și de obicei sunt prelucrate și interpretate folosind reflexia în timpul rulării programului.


## Cum se folosească atributele

Atributele în C# pot fi folosite în următoarele cazuri de utilizare:

- Comunicarea compilatorului C# sau analizorilor Roslyn unele informații semantice asociate cu entitățile particulare din program.
  
  De exemplu, se poate marca un parametru al funcției ca `[NotNull]` (are sens dacă nu se folosește flagul enable nullable) pentru a semnaliza compilatorului și posibil analizorilor că parametru nu poate fi `null`.
  În cazul în care funcției este dat un parametru null, sau o referință care poate fi nulă, compilatorul sau analizorii vor produce o eroare sau un warning.

  Un alt exemplu poate fi atributul `[Flags]` care adaugă metoda `HasFlag` la un anumit enum.

  Încă un exemplu, atributul `[AttributeUsage]` definește la ce grupuri de entități poate fi aplicat anumitul atribut, fie structuri, clase, parametrii unei funcții, membrii unui tip, etc.


- Adăugarea informației suplimentare folosite în timpul rulării de către runtime-ul.
  
  De exemplu, există atributul `[MethodImpl]`, care poate fi folosit pentru a indica JIT-ului dacă o metodă trebuie să fie inliniată, sau trebuie să fie lăsată cum este.

  Atributul `[DLLImport]` indică faptul că o metodă particulară trebuie să fie încărcată din librăria dinamică, numele cărei este dat ca parametru.


- Adăugarea informației generale, analizate de către clasele din librăriile standarte, sau în codul propriu.

  De exemplu, atributul `[Serializable]` indică faptul că un tip poate fi serializat.
  Faptul dat este folosit, de exemplu, de către `BinarySerializer` pentru a determina dacă un tip trebuie sau nu trebuie să fie sălvat.


- Adăugarea informațiilor specifice unei librării, analizate folosind reflexie runtime de către funcțiile acelei librării; folosirea atributelor în codul propriu.

  De exemplu, librăria ASP.NET Core folosește foarte mult reflexia.
  Librăria definește, de exemplu, atributele `[ApiController]`, `[HttpGet]`, `[HttpPost]` etc. pentru definirea endpoint-urilor; `[FromBody]`, `[FromRoute]`, `[FromQuery]` etc. pentru a specifica modul de bindare a parametrilor; `[Email]`, `[Range]`, `[Required]` etc. pentru definirea contractelor tipurilor și realizarea validării.

  Putem defini și atributele proprii, analizându-le în timpul rulării programului folosind reflexia runtime.
  Acest aspect va fi descris mai detaliat mai târziu.


- Configurarea generatorilor de sursă sau a generatorilor de cod personalizate.
  Cazul acesta de utilizare se aseamănă cu folosirea analizorilor Roslyn în ceea că informațiile adăugate la entitățile nu se folosesc în timpul rulării programului, ci în timpul compilării.
  În cazul generatorilor de cod personalizate, sunt folosite chiar înainte de propriu-zisă compilare.

  Ca exemplu pot fi menționate diferitele generatori de sursă care lucrează împreună cu compilatorul C#, ori instrumentele ca MagicOnion, MessagePack, sau generatorul meu de cod Kari care generează codul înainte de compilare pe baza informațiilor din fișierile-sursă.


## Unele exemple minime

Chiar tipurile primitive în C# au câteva atribute care pot fi analizate prin cod.
De exemplu, următorul programul minim afișează trei atribute:

```csharp
foreach (var attr in typeof(int).GetCustomAttributes(inherit: true))
    Console.WriteLine(attr);

// System.SerializableAttribute
// System.Runtime.CompilerServices.IsReadOnlyAttribute
// System.Runtime.CompilerServices.TypeForwardedFromAttribute
```

Sau asamblajul curent (acestea sunt aplicate într-un fișier generat automat):

```csharp
foreach (var attr in typeof(A).Assembly.GetCustomAttributes(inherit: true))
    Console.WriteLine(attr);
class A {}

// System.Runtime.CompilerServices.CompilationRelaxationsAttribute
// System.Runtime.CompilerServices.RuntimeCompatibilityAttribute
// System.Diagnostics.DebuggableAttribute
// System.Runtime.Versioning.TargetFrameworkAttribute
// System.Reflection.AssemblyCompanyAttribute
// System.Reflection.AssemblyConfigurationAttribute
// System.Reflection.AssemblyFileVersionAttribute
// System.Reflection.AssemblyInformationalVersionAttribute
// System.Reflection.AssemblyProductAttribute
// System.Reflection.AssemblyTitleAttribute
```

Încă un exemplu mai complex.
În acest caz creez un atribut personalizat, îl aplic la un alt tip, pe urmă folosesc reflexia pentru a-l lua de pe tip, și afișez valoarea câmpului.

```csharp
using System.Reflection;

// Se definește un atribut personalizat.
class TestAttribute : Attribute
{
    public int FieldValue;

    public TestAttribute(int value)
    {
        FieldValue = value;
    }
}

// Atributul se aplică la clasa A.
[Test(5)]
class A
{
}

class Program
{
    static void Main()
    {
        // Se folosește reflexia runtime pentru a lua atributul nostru de pe tip.
        var attr = typeof(A).GetCustomAttribute<TestAttribute>()!;
        int fieldValue = attr.FieldValue;

        // Se afișează valoarea câmpului.
        Console.WriteLine(fieldValue); // 5
    }
}
```


## Exemplu program - ArgumentParser

> În următorul exemplu se va implementa parsarea argumentelor din linie de comandă.

Orice program poate fi apelat cu unele argumente de tip `string`, date prin linie de comandă la executare.
De exemplu, considerând că programul nostru se numește `program`, dacă se apelează:
- Simplu ca `program`, primește nimic drept argumente;
- `program argument`, primește șirul "argument" ca unicul argument;
- `program --key value`, primește șirurile "--key" și "value" ca argumente.

> În unele limbaje de programare, primul argument în tabloul cu argumente uneori este rezervat pentru numele programului executat.


În exemplul dat se realizează un parser de argumente general cu înțelegerea tipurilor `int` și `string`, care va lucra pe baza input-utilor în formatul `--key value`.

Adică, de exemplu, fie un program cu argumentele `A` și `B`.
Numele argumentelor corespund cheilor, așteptate la intrare de către program.
Fie că tipul așteptat pentru argumentul `A` este `int`, iar pentru `B` este `string`.
Urmează câteva exemple de apelare:

| Executat ca                        | Rezultatul așteptat                                                                  |
|------------------------------------|--------------------------------------------------------------------------------------|
| `program`                          | lipsesc opțiunile `A` și `B`                                                        |
| `program --A 123`                  | lipsește opțiunea `B`                                                                |
| `program --A 123 --B`              | lipsește o valoare pentru opțiunea `B`                                               |
| `program --A 123 --B abc`          | succes, `A` primește valoarea `123`, iar `B` — `"abc"`                               |
| `program --A abc --B abc`          | "abc" nu reprezintă un `int`                                                         |
| `program --C ytyt --A 123 --B abc` | succes (în implementări mai complexe poate da eroarea de "argumentul nefolosit `C`") |


Pentru exemplu în cod, vom folosi opțiunile `Hello` de tip `int` și `OtherName` de tip `string`.
În primul rând, vom defini o clasă pentru a ține valorile tuturor opțiunilor obținute în urma parsării argumentelor, deci vom avea câte un câmp pentru fiecare opțiune.
Am numit al doilea câmp `Stuff`, adică în cod ne vom referi la el folosind numele `Stuff`, însă, mai departe vom face niște schimbări ca să se adreseze la el după numele `OtherName` din perspectiva utilizatorului care folosește aplicația din linie de comandă.

> Acest concept, unde numele unei entități în interfață (pentru utilizator) este diferit decât numele aceiași entități folosit intern, în cod, se mai numește "display name".

```csharp
class ArgumentModel
{
    public int Hello;
    public string? Stuff;
}
```


Acum vom descrie funcția `Main`, cum dorim să folosim metodele de parsare a argumentelor din linie de comandă în modelul de argumente (`ArgumentModel`). 

```csharp
class Program
{
    static int Main(string[] args)
    {
        // Se încearcă a converta argumentele din linie de comandă în modelul de argumente.
        if (!ArgumentParser.TryParse(args, out ArgumentModel? model))
        {
            // Dacă convertarea a eșuat, ar dori să se afișeze mesajul de ajutor.
            var builder = new StringBuilder();
            ArgumentParser.GetHelp<ArgumentModel>(builder);
            Console.WriteLine(builder.ToString());
            return 1;
        }

        // Convertarea a lucrat, afișăm valorile opțiunilor.
        Console.WriteLine("Hello = " + model.Hello);
        Console.WriteLine("Stuff = " + model.Stuff);
        return 0;
    }
}
```

Clar că pentru a putea defini un nume diferit decât numele câmpului, afișa un mesaj de ajutor personalizat, și ca funcția `ArgumentParser.TryParse` să fie atât de generală ca să poată lucra cu orice tip, fără a o supraîncărca pentru orice tip nou de modelul de argumente care vrem să-l suportăm, trebuie, în primul rând, s-o facem generică, ca să cunoască despre tipul în care va fi realizată conversiunea, și trebuie să atribuim informații adaugătoare cu numele opțiunii și mesajul de ajutor la fiecare câmp ce urmează să țină o valoare pentru opțiunea specifică.
Putem adăuga niște informații direct la câmpuri, folosind un atribut personalizat.
Vom defini un tip nou `OptionAttribute` care să țină câte o valoare pentru numele opțiunii și mesajul de ajutor.
Permitem doar apelarea la un câmp, folosind atributul `AttributeUsage`.
Deoarece un nume nou este opțional, definesc două constructor, unul cu nume, altul fără.

```csharp
[AttributeUsage(AttributeTargets.Field)]
class OptionAttribute : Attribute
{
    public string? Name;
    public string HelpMessage;

    public OptionAttribute(string helpMessage)
    {
        HelpMessage = helpMessage;
    }

    public OptionAttribute(string name, string helpMessage)
    {
        Name = name;
        HelpMessage = helpMessage;
    }
}
```

După ce adaugăm aplicarea atributelor la câmpurile ce să reprezinte opțiunile: 

```csharp
class ArgumentModel
{
    // Se apelează primul constructor (fără nume, doar mesajul)
    [Option("Help message for Hello")]
    public int Hello;

    // Se apelează al doilea constructor (numele, mesajul)
    [Option("OtherName", "A renamed option")]
    public string? Stuff;
}
```


Acum vom implementa funcția generică `TryParse`.

```csharp
public static class ArgumentParser
{
    // Această funcție returnează un `bool`, indicând dacă convertarea a avut succes,
    // și o valoare de tipul de model dorit, returnată printr-un parametru `out`.
    // `T` reprezintă tipul modelulul.
    public static bool TryParse<T>(
        
        // Un span cu argumentele, de fapt un tablou imuabil.
        ReadOnlySpan<string> args,
        
        // Parametrul nu va conține null dacă valoarea returnată este true.
        [NotNullWhen(returnValue: true)] out T? result)

        // Vom avea nevoie să creăm o instanță nouă de T. 
        where T : new()
    {
    }
}
```

Acum vom discuta logica funcționării acestei funcții:
- Se vor parsa argumentele, detectând pattern-ul de formă `--key value` și se va crea o mapare de aceștia (un tablou asociativ `key -> value`);
- Se vor detecta câmpurile tipului modelului care vor conține valorile pentru opțiuni;
- Pentru fiecare câmp, se va încerca a-i găsi o valoare asociată din tabloul asociativ, obținut anterior;
- Se va încerca a converta șirul crud păstrat în tabelul asociativ în tipul concret al câmpului.
- Se vor atribui valorile concrete obținute după parsare la câmpuri asociate;
- Dacă nu au avut loc greșeli, vom returna obiectul final.

```csharp
// Se atribuie valoarea `null` dacă modelul este o clasă,
// sau o structură inițializată implicit, dacă modelul este o structură.
result = default;

// --name value
Dictionary<string, string> values = new();
for (int i = 0; i < args.Length; i++)
{
    if (args[i].StartsWith("--"))
    {
        // "--key" -> "key"
        var name = args[i]["--".Length ..];
        
        // Următorul argument reprezintă valoarea.
        i++;
        if (i >= args.Length)
        {
            Console.WriteLine("Argument without value: " + name);
            return false;
        }
        var value = args[i];

        // Se adaugă asocierea în tabloul asociativ.
        values[name] = value;
        continue;
    }

    Console.WriteLine("Error at " + args[i] + ". Start your arguments with '--'.");
    return false;
}

// Vom folosi un flag pentru a putea arăta mai multe erori.
bool isError = false;
var obj = new T();

// Se folosește reflexia runtime pentru a primi informații despre câmpurile tipului.
foreach (var field in typeof(T).GetFields())
{
    // Ne interesează câmpurile cu atributul [Option], altfel le ignorăm.
    var optionAttribute = field.GetCustomAttribute<OptionAttribute>();
    if (optionAttribute is null)
        continue;

    // Numele implicit este numele câmpului, îl folosim dacă unul nu este dat explicit.
    var optionName = optionAttribute.Name ?? field.Name;
    if (!values.TryGetValue(optionName, out var strValue))
    {
        isError = true;
        Console.WriteLine("No value found for option " + optionName);
        continue;
    }
    
    // Evităm atribuirea valorilor.
    if (isError)
        continue;

    // Suportăm doar două tipuri: `int` și `string`.
    if (field.FieldType == typeof(int))
    {
        if (!int.TryParse(strValue, out int v))
        {
            Console.WriteLine("Could not parse " + strValue + " as int.");
            isError = true;
            continue;
        }
        field.SetValue(obj, v);
    }
    else if (field.FieldType == typeof(string))
    {
        field.SetValue(obj, strValue);
    }
    else
    {
        isError = true;
        Console.WriteLine("Unsupported type: " + field.FieldType.FullName);
    }
}

if (isError)
    return false;

result = (T) obj;
return true;
```

Rămâne să definim metoda de obținere a mesajului de ajutor.
Am implementat și metoda `GetFieldsWithOptions` pentru a evita repetarea codului (am substituit și codul corespunzător la invocarea acestei funcții și în funcția TryParse). 

```csharp
public static class ArgumentParser
{
    // ...

    internal static IEnumerable<(FieldInfo, OptionAttribute)> GetFieldsWithOptions(System.Type type)
    {
        foreach (var field in type.GetFields())
        {
            var optionAttribute = field.GetCustomAttribute<OptionAttribute>();
            if (optionAttribute is null)
                continue;
            
            yield return (field, optionAttribute);
        }
    }

    public static void GetHelp<T>(StringBuilder builder)
    {
        foreach (var (field, optionAttribute) in GetFieldsWithOptions(typeof(T)))
        {
            builder.Append(optionAttribute.Name ?? field.Name);
            builder.Append(", tipul '");
            builder.Append(field.FieldType.Name);
            builder.Append("', help '");
            builder.Append(optionAttribute.HelpMessage);
            builder.Append("'");
            builder.AppendLine();
        }
    }
}
```


Unele exemple de invocare:
```
$ dotnet run --Hello 123 --OtherName abc           
Hello = 123                                        
Stuff = abc                                        
                                            
$ dotnet run --Hello 123                           
No value found for option OtherName                
Hello, tipul 'Int32', help 'Help message for Hello'
OtherName, tipul 'String', help 'A renamed option' 
                                            
$ dotnet run --Hello abc --OtherName 123           
Could not parse abc as int.                        
Hello, tipul 'Int32', help 'Help message for Hello'
OtherName, tipul 'String', help 'A renamed option' 
                                            
$ dotnet run --A a --B b                           
No value found for option Hello                    
No value found for option OtherName                
Hello, tipul 'Int32', help 'Help message for Hello'
OtherName, tipul 'String', help 'A renamed option' 
                                            
$ dotnet run --OtherName                           
Argument without value: OtherName                  
Hello, tipul 'Int32', help 'Help message for Hello'
OtherName, tipul 'String', help 'A renamed option' 
```