using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoCode
{
    public static class NekodeSystemSkills
    {
        public static Variable DoSkill(Variable v, dynamic p)
        {
            if (v.Value == "__LIST_SKILL__") // List default skills
            {
                if (v.Name == "roll")
                {
                    return ListDefaultSkills.roll(v, p);
                }
                if (v.Name == "map")
                {
                    return ListDefaultSkills.map(v, p);
                }
                if (v.Name == "clear")
                {
                    return ListDefaultSkills.clear(v, p);
                }
                if (v.Name == "get")
                {
                    return ListDefaultSkills.clear(v, p);
                }
                if (v.Name == "find")
                {
                    return ListDefaultSkills.find(v, p);
                }

            }
            if (v.Value == "me") //addon "me"
            {
                if (v.Name == "say")
                {
                    return NekoAddons.me.me.say(v, p);
                }
                if (v.Name == "hear")
                {
                    return NekoAddons.me.me.hear(v, p);
                }
                if (v.Name == "pat")
                {
                    return NekoAddons.me.me.pat(v, p);
                }
                if (v.Name == "command")
                {
                    return NekoAddons.me.me.command(v, p);
                }
                if (v.Name == "display")
                {
                    return NekoAddons.me.me.display(v, p);
                }
                if (v.Name == "compile")
                {
                    return NekoAddons.me.me.compile(v, p);
                }
                if (v.Name == "eat")
                {
                    return NekoAddons.me.me.eat(v, p);
                }
                //if (v.Name == "web")
                //{
                //    return NekoAddons.me.me.web(v, p);
                //}
                if (v.Name == "list")
                {
                    return NekoAddons.me.me.list(v, p);
                }
            }
            if (v.Value == "system") //addon "system"
            { 
                if (v.Name == "run_dialogue")
                {
                    return NekoAddons.system.systemAddon.run_dialogue(v, p);
                }
                if (v.Name == "wait_for_console_input")
                {
                    return NekoAddons.system.systemAddon.wait_for_console_input(v, p);
                }
                if (v.Name == "show_hint")
                {
                    return NekoAddons.system.systemAddon.show_hint(v, p);
                }
                if (v.Name == "hide_hint")
                {
                    return NekoAddons.system.systemAddon.hide_hint(v, p);
                }
            }
            return NekodeTask.NewUnknownVariable();
        }
    }
}
