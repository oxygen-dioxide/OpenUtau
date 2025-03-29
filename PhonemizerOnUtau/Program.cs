// See https://aka.ms/new-console-template for more information
using System.Globalization;
using System.Reflection;
using System.Text;

using utauPlugin;

using OpenUtau.Api;
using OpenUtau.Core;
using OpenUtau.Core.Format;
using OpenUtau.Core.Ustx;

using PhonemizerOnUtau;
using OpenUtau.Classic;

try {
    //Print application version
    string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
    Console.WriteLine($"PhonemizerOnUtau {version}");

    //initialization
    Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
    Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    var arg = Environment.GetCommandLineArgs();

    //Check command line arguments
    if (arg.Length < 2)
    {
        Console.WriteLine("Please launch this plugin from utau editor");
        Console.ReadLine();
        return;
    }

    //Load phonemizers
    var phonemizerFactories = PhonemizerLoader.LoadAllPhonemizers()?.ToList() ?? null;
    
    if(phonemizerFactories == null || phonemizerFactories.Count == 0) {
        Console.WriteLine("No phonemizer found. Please put the phonemizer dlls in the \"plugins\" folder under PhonemizerOnUtau.");
        Console.ReadLine();
        return;
    }

    //Load tmp ust project
    UtauPlugin plugin = new UtauPlugin(arg[1]);
    plugin.Input();

    //Load voicebank
    var singerPath = plugin.VoiceDir;
    Console.WriteLine($"Singer directory: {singerPath}");
    var voicebank = new Voicebank() { 
        File = Path.Combine(singerPath, "character.txt"), 
        BasePath = Path.Combine(singerPath, "..") 
    };
    VoicebankLoader.LoadVoicebank(voicebank);
    var singer = new ClassicSinger(voicebank);
    singer.EnsureLoaded();

    //Check if the user has selected any note
    if(IsEmptyProject(plugin)) {
        Console.WriteLine("Please select some notes before running this plugin.");
        Console.WriteLine("Press Enter to exit.");
        Console.ReadLine();
        return;
    }

    //Parse notes and tempos
    var myNotes = UtauUtil.PluginNotesToMyNotes(plugin);
    var tempos = UtauUtil.GetTempoList(plugin, myNotes);
    var timeAxis = new TimeAxis();
    timeAxis.BuildSegments(new UProject() { tempos = tempos });
    Dictionary<int, MyNote> NoteByPosition = myNotes.ToDictionary(n => n.position);

    //Ask the user which phonemizer to use
    var phonemizerGroups = phonemizerFactories.GroupBy(factory => factory.language)
        .OrderBy(group => group.Key)
        .ToList();
    PhonemizerFactory phonemizerSelected = null;
    bool userSelect = false;
    while (!userSelect) {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Phonemizer languages:");
        Console.ForegroundColor= ConsoleColor.White;
        foreach(var g in phonemizerGroups) {
            Console.WriteLine($"{g.Key ?? "General"}");
        }
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Please input a phonemizer language, or the tag of a phonemizer:");
        Console.ForegroundColor = ConsoleColor.White;
        string input = Console.ReadLine()?.Trim().ToLower() ?? "";
        if (TryParsePhonemizerGroup(phonemizerGroups, input, out var phonemizerGroup)) {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Phonemizer for language {phonemizerGroup.Key}");
            Console.ForegroundColor = ConsoleColor.White;
            foreach(var p in phonemizerGroup) {
                Console.WriteLine($"{p.ToString()}");
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Please input the tag of a phonemizer:");
            Console.ForegroundColor = ConsoleColor.White;
            input = Console.ReadLine()?.Trim().ToLower() ?? "";
        }
        if (TryParsePhonemizer(phonemizerFactories, input, out var phonemizerParsed)) {
            phonemizerSelected = phonemizerParsed;
            userSelect = true;
            continue;
        }
    }
    Console.WriteLine($"Using phonemizer {phonemizerSelected.ToString()}");

    //Prepare phonemizer input
    var noteIndexes = new List<int>();
    var groupList = new List<Phonemizer.Note[]>() { };
    List<MyNote> currentGroup = new List<MyNote>() { };
    foreach (var note in myNotes) {
        if (note.IsSlur()) {
            //Add slur note into the current group
            currentGroup.Add(note);
        } else {
            //End the previous group and start a new one
            if (currentGroup.Count > 0) {
                groupList.Add(currentGroup.Select(n => n.ToPhonemizerNote()).ToArray());
            }
            currentGroup.Clear();
            currentGroup.Add(note);
        }
    }
    if(currentGroup.Count > 0) {
        groupList.Add(currentGroup.Select(n => n.ToPhonemizerNote()).ToArray());
    }
    var groupArray = groupList.ToArray();

    //Set up dummy project
    var project = new OpenUtau.Core.Ustx.UProject();
    Ustx.AddDefaultExpressions(project);
    var track = project.tracks[0];
    project.expressions.TryGetValue(Ustx.CLR, out var descriptor);
    track.VoiceColorExp = descriptor.Clone();
    var colors = singer.Subbanks.Select(subbank => subbank.Color).ToHashSet();
    track.VoiceColorExp.options = colors.OrderBy(c => c).ToArray();
    track.VoiceColorExp.max = track.VoiceColorExp.options.Length - 1;

    //Run phonemizer
    Phonemizer phonemizer = phonemizerSelected.Create();
    phonemizer.Testing = true;
    phonemizer.SetSinger(singer);
    phonemizer.SetTiming(timeAxis);
    var resultNotes = new List<MyNote>() { };
    var currTick = 0;
    phonemizer.SetUp(groupArray, project, track);
    for (var i = 0; i < groupArray.Length; i++) {
        var group = groupArray[i];
        var result = phonemizer.Process(
            groupArray[i],
            i > 0 ? groupArray[i - 1].First() : null,
            i < groupArray.Length - 1 ? groupArray[i + 1].First() : null,
            (i > 0 && IsNeighbor(groupArray[i - 1], groupArray[i])) ? groupArray[i - 1].First() : null,
            (i < groupArray.Length - 1 && IsNeighbor(groupArray[i], groupArray[i + 1])) ? groupArray[i + 1].First() : null,
            i > 0 ? groupArray[i - 1] : null);
        foreach(var phoneme in result.phonemes) {
            //ensure each phoneme is at least 5 ticks
            int position = Math.Max(phoneme.position + group[0].position, currTick + 5);
            int duration = Math.Max(group[^1].position + group[^1].duration - position, 5);
            var currNote = new MyNote() {
                position = position,
                duration = duration,
                lyric = phoneme.phoneme,
                tone = NoteByPosition[group[0].position].tone,
                flags = NoteByPosition[group[0].position].flags,
            };
            resultNotes.Add(currNote);
        }
    }
    //Fix overlapping notes
    foreach (var pair in resultNotes.Zip(resultNotes.Skip(1))) {
        if(pair.First.end > pair.Second.position) {
            pair.First.duration = pair.Second.position - pair.First.position;
        }
    }
    //Determine tone for each phoneme
    //Currently we use the middle point on time axis of each phoneme.
    //If it belongs to a note's time range, the phoneme use the note's tone
    //otherwise it use the note it belongs to.
    int inputNoteIndex = 0;
    foreach (var resultNote in resultNotes) {
        int middlePoint = resultNote.position + resultNote.duration / 2;
        while (inputNoteIndex < myNotes.Count && myNotes[inputNoteIndex].end < middlePoint) {
            inputNoteIndex++;
        }
        if (inputNoteIndex >= myNotes.Count) {
            break;
        }
        if (myNotes[inputNoteIndex].position <= middlePoint && myNotes[inputNoteIndex].end >= middlePoint) {
            resultNote.tone = myNotes[inputNoteIndex].tone;
            resultNote.flags = myNotes[inputNoteIndex].flags;
        }
    }
    if(resultNotes.Count == 0) {
        return;
    }
    //Write to tmp ust
    int originalPluginNoteCount = plugin.note.Count;
    bool hasPrev = plugin.note[0].GetNum() == "PREV";
    bool hasNext = plugin.note[^1].GetNum() == "NEXT";
    int insertLocation = hasPrev ? 1 : 0;
    foreach (var pluginNoteIndex in Enumerable.Range(insertLocation, originalPluginNoteCount - insertLocation - (hasNext ? 1 : 0))) {
        plugin.DeleteNote(pluginNoteIndex);
    }
    currTick = 0;
    if (hasPrev) {
        plugin.note[0].SetLength(resultNotes[0].position);
        currTick = resultNotes[0].position;
    }
    foreach(var resultNote in resultNotes) {
        if(resultNote.position > currTick) {
            //Insert R note
            InsertPluginRest(resultNote.position - currTick, plugin, insertLocation);
            insertLocation++;
        }
        InsertPluginNote(
            resultNote.duration, 
            resultNote.tone, 
            resultNote.lyric, 
            resultNote.flags, 
            plugin, 
            insertLocation);
        insertLocation++;
        currTick = resultNote.end;
    }
    plugin.Output();
} catch(Exception e) {
    Console.WriteLine(e);
    Console.WriteLine("Press Enter to exit.");
    Console.ReadLine();
    return;
}

bool IsEmptyProject(UtauPlugin plugin){
    if(plugin.note.Count == 0){
        return true;
    }
    bool hasPrev = plugin.note[0].GetNum() == "PREV";
    bool hasNext = plugin.note[^1].GetNum() == "NEXT";
    if(plugin.note.Count - (hasPrev ? 1 : 0) - (hasNext ? 1 : 0) == 0) {
        return true;
    }
    return false;
}

bool TryParsePhonemizerGroup(
    List<IGrouping<string, PhonemizerFactory>> phonemizerGroups, 
    string input, 
    out IGrouping<string, PhonemizerFactory>? phonemizerGroup) {
        phonemizerGroup = phonemizerGroups.FirstOrDefault(g => (g.Key ?? "general").ToLower() == input);
    if(phonemizerGroup != null) {
        return true;
    }
    return false;
}

bool TryParsePhonemizer(
    List<PhonemizerFactory> phonemizers,
    string input,
    out PhonemizerFactory? phonemizer) {
    phonemizer = phonemizers.FirstOrDefault(p => p.tag.ToLower() == input);
    if(phonemizer != null) {
        return true;
    }
    return false;
}

bool IsNeighbor(Phonemizer.Note[] prevGroup, Phonemizer.Note[] currGroup) {
    return prevGroup[^1].position + prevGroup[^1].duration == currGroup[0].position;
}

void InsertPluginNote(
    int duration, 
    int tone, 
    string lyric, 
    string flags,
    UtauPlugin plugin, 
    int insertLocation) {
    plugin.InsertNote(insertLocation);
    plugin.note[insertLocation].SetLength(duration);
    plugin.note[insertLocation].SetNoteNum(tone);
    plugin.note[insertLocation].SetLyric(lyric);
    plugin.note[insertLocation].SetFlags(flags);
}

void InsertPluginRest(
    int duration,
    UtauPlugin plugin,
    int insertLocation){
    InsertPluginNote(
        duration,
        60,
        "R",
        "",
        plugin,
        insertLocation);
}
