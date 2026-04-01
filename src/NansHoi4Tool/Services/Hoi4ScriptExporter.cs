using NansHoi4Tool.Models;
using System.Text;

namespace NansHoi4Tool.Services;

public interface IHoi4ScriptExporter
{
    string ExportFocusTree(FocusTreeData data);
    string ExportEvents(EventsData data);
    string ExportIdeas(IdeasData data);
    string ExportDecisions(DecisionsData data);
    string ExportTechnologies(TechData data);
    string ExportLocalisation(LocalisationData data, string language = "english");
}

public class Hoi4ScriptExporter : IHoi4ScriptExporter
{
    public string ExportFocusTree(FocusTreeData data)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"focus_tree = {{");
        sb.AppendLine($"\tid = {data.TreeId}");
        if (!string.IsNullOrWhiteSpace(data.CountryTag))
        {
            sb.AppendLine($"\tcountry = {{");
            sb.AppendLine($"\t\tfactor = 0");
            sb.AppendLine($"\t\tmodifier = {{");
            sb.AppendLine($"\t\t\tadd = 10");
            sb.AppendLine($"\t\t\ttag = {data.CountryTag}");
            sb.AppendLine($"\t\t}}");
            sb.AppendLine($"\t}}");
        }
        if (data.Continuous) sb.AppendLine($"\tis_default = no");
        sb.AppendLine();

        foreach (var node in data.Nodes)
        {
            sb.AppendLine($"\tfocus = {{");
            sb.AppendLine($"\t\tid = {node.Id}");
            sb.AppendLine($"\t\ticon = {node.Icon}");
            sb.AppendLine($"\t\tcost = {node.Cost}");
            sb.AppendLine($"\t\tx = {node.X}");
            sb.AppendLine($"\t\ty = {node.Y}");

            foreach (var prereq in node.Prerequisites)
                sb.AppendLine($"\t\tprerequisite = {{ focus = {prereq} }}");

            foreach (var mutex in node.MutuallyExclusive)
                sb.AppendLine($"\t\tmutually_exclusive = {{ focus = {mutex} }}");

            if (node.CancelIfInvalid) sb.AppendLine($"\t\tcancel_if_invalid = yes");
            if (node.ContinueIfInvalid) sb.AppendLine($"\t\tcontinue_if_invalid = yes");
            if (node.AvailableIfCapitulated) sb.AppendLine($"\t\tavailable_if_capitulated = yes");

            if (!string.IsNullOrWhiteSpace(node.Available))
            {
                sb.AppendLine($"\t\tavailable = {{");
                foreach (var line in node.Available.Split('\n'))
                    sb.AppendLine($"\t\t\t{line.Trim()}");
                sb.AppendLine($"\t\t}}");
            }

            sb.AppendLine($"\t\tcompletion_reward = {{");
            if (!string.IsNullOrWhiteSpace(node.CompletionReward))
                foreach (var line in node.CompletionReward.Split('\n'))
                    sb.AppendLine($"\t\t\t{line.Trim()}");
            sb.AppendLine($"\t\t}}");
            sb.AppendLine($"\t}}");
            sb.AppendLine();
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    public string ExportEvents(EventsData data)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"add_namespace = {data.Namespace}");
        sb.AppendLine();

        foreach (var ev in data.Events)
        {
            sb.AppendLine($"{ev.Type} = {{");
            sb.AppendLine($"\tid = {ev.Id}");
            sb.AppendLine($"\ttitle = {ev.Id}.t");
            sb.AppendLine($"\tdesc = {ev.Id}.d");
            sb.AppendLine($"\tpicture = {ev.Picture}");
            sb.AppendLine();

            if (ev.IsTriggeredOnly) sb.AppendLine("\tis_triggered_only = yes");
            if (ev.Hidden) sb.AppendLine("\thidden = yes");
            if (ev.FireOnlyOnce) sb.AppendLine("\tfire_only_once = yes");

            if (!string.IsNullOrWhiteSpace(ev.Trigger))
            {
                sb.AppendLine($"\ttrigger = {{");
                foreach (var line in ev.Trigger.Split('\n'))
                    sb.AppendLine($"\t\t{line.Trim()}");
                sb.AppendLine($"\t}}");
            }

            if (!string.IsNullOrWhiteSpace(ev.MeanTimeToHappen))
            {
                sb.AppendLine($"\tmean_time_to_happen = {{");
                foreach (var line in ev.MeanTimeToHappen.Split('\n'))
                    sb.AppendLine($"\t\t{line.Trim()}");
                sb.AppendLine($"\t}}");
            }

            sb.AppendLine();
            foreach (var opt in ev.Options)
            {
                sb.AppendLine($"\toption = {{");
                sb.AppendLine($"\t\tname = {opt.Name}");
                if (!string.IsNullOrWhiteSpace(opt.Trigger))
                {
                    sb.AppendLine($"\t\ttrigger = {{");
                    foreach (var line in opt.Trigger.Split('\n'))
                        sb.AppendLine($"\t\t\t{line.Trim()}");
                    sb.AppendLine($"\t\t}}");
                }
                if (!string.IsNullOrWhiteSpace(opt.Effect))
                    foreach (var line in opt.Effect.Split('\n'))
                        sb.AppendLine($"\t\t{line.Trim()}");
                sb.AppendLine($"\t}}");
            }

            sb.AppendLine("}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public string ExportIdeas(IdeasData data)
    {
        var sb = new StringBuilder();
        sb.AppendLine("ideas = {");
        var bySlot = data.Ideas.GroupBy(i => i.Slot);
        foreach (var slot in bySlot)
        {
            sb.AppendLine($"\t{slot.Key} = {{");
            foreach (var idea in slot)
            {
                sb.AppendLine($"\t\t{idea.Id} = {{");
                sb.AppendLine($"\t\t\tpicture = {idea.Picture}");
                if (idea.AllowedCivilWar) sb.AppendLine($"\t\t\tallowed_civil_war = {{ always = yes }}");
                if (!string.IsNullOrWhiteSpace(idea.Available))
                    sb.AppendLine($"\t\t\tavailable = {{ {idea.Available} }}");
                if (idea.Modifiers.Count > 0)
                {
                    sb.AppendLine($"\t\t\tmodifier = {{");
                    foreach (var mod in idea.Modifiers)
                        sb.AppendLine($"\t\t\t\t{mod.Key} = {mod.Value}");
                    sb.AppendLine($"\t\t\t}}");
                }
                sb.AppendLine($"\t\t}}");
            }
            sb.AppendLine($"\t}}");
        }
        sb.AppendLine("}");
        return sb.ToString();
    }

    public string ExportTechnologies(TechData data)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"technologies = {{  # {data.CategoryId}");
        foreach (var tech in data.Technologies)
        {
            sb.AppendLine($"\t{tech.Id} = {{  # start_year = {tech.StartYear}");
            sb.AppendLine($"\t\tresearch_cost = {tech.ResearchCost}");
            sb.AppendLine($"\t\tstart_year = {tech.StartYear}");
            if (!string.IsNullOrWhiteSpace(tech.Folder))
                sb.AppendLine($"\t\tfolder = {{ name = {tech.Folder} position = {{ x = {(int)tech.X} y = {(int)tech.Y} }} }}");
            foreach (var prereq in tech.Prerequisites)
                sb.AppendLine($"\t\tdependencies = {{ {prereq} }}");
            if (tech.Bonuses.Count > 0)
            {
                sb.AppendLine($"\t\tcategories = {{");
                foreach (var bonus in tech.Bonuses)
                    sb.AppendLine($"\t\t\t{bonus.Key} = {bonus.Value}");
                sb.AppendLine($"\t\t}}");
            }
            sb.AppendLine($"\t}}");
            sb.AppendLine();
        }
        sb.AppendLine("}");
        return sb.ToString();
    }

    public string ExportDecisions(DecisionsData data)
    {
        var sb = new StringBuilder();
        foreach (var cat in data.Categories)
        {
            sb.AppendLine($"{cat.Id} = {{");
            if (!string.IsNullOrWhiteSpace(cat.Icon)) sb.AppendLine($"\ticon = {cat.Icon}");
            sb.AppendLine();
            foreach (var d in cat.Decisions)
            {
                sb.AppendLine($"\t{d.Id} = {{");
                sb.AppendLine($"\t\ticon = {d.Icon}");
                sb.AppendLine($"\t\tcost = political_power");
                sb.AppendLine($"\t\tcost = {d.CostPoliticalPower}");
                if (d.DaysReDo > 0) sb.AppendLine($"\t\tdays_re_do = {d.DaysReDo}");
                if (d.DaysRemove > 0) sb.AppendLine($"\t\tdays_remove = {d.DaysRemove}");
                if (!string.IsNullOrWhiteSpace(d.Available))
                {
                    sb.AppendLine($"\t\tavailable = {{");
                    foreach (var line in d.Available.Split('\n'))
                        sb.AppendLine($"\t\t\t{line.Trim()}");
                    sb.AppendLine($"\t\t}}");
                }
                if (!string.IsNullOrWhiteSpace(d.Effect))
                {
                    sb.AppendLine($"\t\tcomplete_effect = {{");
                    foreach (var line in d.Effect.Split('\n'))
                        sb.AppendLine($"\t\t\t{line.Trim()}");
                    sb.AppendLine($"\t\t}}");
                }
                sb.AppendLine($"\t}}");
                sb.AppendLine();
            }
            sb.AppendLine("}");
            sb.AppendLine();
        }
        return sb.ToString();
    }

    public string ExportLocalisation(LocalisationData data, string language = "english")
    {
        var sb = new StringBuilder();
        sb.AppendLine($"l_{language}:");
        foreach (var entry in data.Entries)
        {
            var value = language switch
            {
                "french" => entry.French,
                "german" => entry.German,
                "spanish" => entry.Spanish,
                "russian" => entry.Russian,
                "polish" => entry.Polish,
                _ => entry.English
            };
            sb.AppendLine($" {entry.Key}:0 \"{value}\"");
        }
        return sb.ToString();
    }
}
