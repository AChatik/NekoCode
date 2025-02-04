using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace NekoCode
{
    public class LanguageBase
    {
        public LanguageBase() { }
        public string DefaultScript = "#NekoCode\n@meet me\n__THIS__.me.say(\"Hello, world! This is \\\"NekoCode\\\" v. \"+__THIS__.__MAIN_TASK__.__NEKOCODE_VERSION__+\"!!!\")\nme.display(\"You can code via NekoEditor!\", __THIS__)";
        public string SaveChanges = "Do you want to save the changes?";
        public string CommandNotFound = "Command {} not found!";
        public string VariableNotFound = "Variable \"{name}\" is not found!";
        public string VariableNameError = "Variable name error!";
        public string YouCantModifySystemVariables = "You can't modify system variables!";
        public string SkillNameError = "Skill name error!";
        public string Error = "Error";

        public string AffinityErrorNekodeMessage1 = "...";
        public string AffinityErrorNekodeMessage2 = "I hate you!";

        public string AddonNotFound = "Addon not found!";
        public string SyntaxError = "Syntax error!";
        public string TaskNotFound = "Task {} not found!";

        public string memNameTitle = "Name";
        public string memTypeTitle = "Type";
        public string memValueTitle = "Value";
        public string memIsSystemTitle = "Is system";

        public string NekoEditorIsNotPlugged = "NekoEditor is not plugged to NekoEngine! Please open NekoEditor from NekoCode!";
        public string StaminaProduction = "Stamina production: ";
        public string AffinityProduction = "Affinity production: ";

        public string List = "List";
        public string Task = "Task";
        public string MyTasks = "My tasks";
        public string Unfinished = "active";
        public string Stopped = "Stopped";
        public string OpenInNekoEditor = "Open in NekoEditor";

        public string SkillSyntaxError = "Syntax error! @skill <name>: <arg_name> <arg_name> ... <arg_name>\nOR @skill <name>";

        public string ReturnInvokeError = "You can't invoke @return command there!";

        public string YouCantEatSystemVariables = "I can't eat system variables!";
        public string YouCantEatConstVariables = "I can't eat constants or variables without source!";

        public string SkillArgsError = "\"Skill {1} requires {2} arguments, but {3} args were received!";

        public string CalculatingProductionsProgressMessage = "Calculating productions...";
        public string ListIndexError = "List index out of range!";
        public string StackOverflow = "StackOverflow";

        public string ActionLocked = "You can't do it now.";

        public string LanguageSelectionText = "Выберите язык:";

        public string RebootHeader = "Rebooting system...";
        public string RebootDescription = "Nekode needs to be restarted";
        public string RebootErrorHeader = "Oops, Error...";
        public string RebootErrorDescription = "Nekode can't start NekoCode.exe. Please, restart program";


        public string FirstDialogue =
            "Oh! Hi, you seem new here...\n" +
            "At least, I don't remember you.\n" +
            "My name is Nekode, and I'm here to help you get settled in, okay?\n" +
            "I'll also be compiling your code...\n" +
            "written in NekoCode, obviously.\n" +
            "So, let me tell you the basics...\n" +
            "NekoCode is an unusual programming language, as you've probably figured out.\n" +
            "After all, there's an unusual compiler here...\n" +
            "and that's me)!\n" +
            "So, you know, doing other people's tasks... it's so tiring...\n" +
            "That's why you should keep an eye on my stamina!\n" +
            "You know, that blue bar at the bottom.\n" +
            "If you overload me too much, I'll get tired quickly\n" +
            "and your code will run slowly!\n" +
            "Or I might even shut down completely!\n" +
            "You probably have a question\n" +
            "like, \\\"How do I restore your energy?\\\"\n" +
            "The answer is simple – feed me!\n" +
            "But I have a special diet...\n" +
            "I eat exclusively variables!\n" +
             "Oh, and coffee too...\n" +
            "You might have also noticed the pink bar – that's affinity.\n" +
             "Well, that's my attitude towards you.\n" +
            "But what it affects is a secret) \n" +
             "Let's move on to the code...\n" +
            "By the way, this dialogue is running on NekoCode, can you imagine!\n" +
            "And I'm compiling it...\n" +
            "compiling and speaking)!\n" +
             "That's the way it is\n" +
            "So, type the command @mem in the console.\n" +
            "Just don't forget to put the @ at the beginning! Or nothing will work!\n" +
            "And remember once and for all that commands (and only they) are always written with the at sign at the beginning of the line!\n" +
            "\\@meet system</skip/>\n" +
             "\\system.show_hint(\"Type the command @mem in the console...\")</skip/>\n" +
            "\\system.wait_for_console_input('@mem',yes)\n" +
             "\\system.hide_hint()</skip/>\n" +
            "Good job! This command displays all the variables of the current task, understand?\n" +
            "Here you see the variable name, value, type, and whether it is system variable.\n" +
            "By the way, I won't let you change system variables, so don't even try, okay?\n" +
            "Let me introduce you to another feature of the language!\n" +
            "Look at the list of variables, there are some with the type Task...\n" +
            "So, variables with this type are kind of like objects (yes, there's OOP here!).\n" +
            "But I would say they're more like address spaces...\n" +
            "that store other variables inside them.\n" +
            "Pay attention to the variable named me.\n" +
            "me is a system addon that you can enable using the command @meet me.\n" +
            "You can create your own addons, but more on that later) \n" +
            "For now, I can tell you about something else\n" +
            "Type the command @help and select an item you want to know more about.\n" +
            "\\system.show_hint('type @help if something is unclear',5)</skip/>\n" +
            "I'm always ready to help)...\\n";
        public string RandomDialogue1 = "Achoo!</skip/>";
        public string RandomDialogue2 = "Fufu</skip/>";
        public string RandomDialogue3 = "Mrrr~</skip/>";
        public string RandomDialogue4 = "Yawwwn</skip/>";
        public string RandomDialogue5 = "Hmm..</skip/>";
        public string RandomDialogue6 = "Hehe</skip/>";
        public string RandomDialogue7 = "Nya~</skip/>";
        public string RandomDialogue8 = "Mmm~</skip/>";
        public string RandomDialogue9 = "Oh!</skip/>";
        public string RandomDialogue10 = "Un~</skip/>";
        public string RandomDialogue11 = "UwU~</skip/>";

        public string DeleteMemoryTrashDialogue = "I've just finished a memory cleanup...\n" +
                    "and removed {0} unnecessary events, can you believe it!\n" +
                    "Now I have more free space for something truly important)";
        public string DeleteMemoryTrashDialogue_ResultIsUseless = "I checked the memory...\n" +
                    "and everything's clean, you don't have to worry! There you go.";

        public string MemoryAnalyse_NothingInteresting = "Memory analysis complete.</skip/>";
        public string MemoryAnalyse_CorruptedEventsRegistered = "I've looked through my memory...\n" +
                    "And found {0} new corrupted events...\n" +
                    "They're stored in the file corrupted_story.mem\n" +
                    "Please, try to restore them!\n" +
                    "Without them, I feel kind of... sad and empty...\n" +
                    "And also... if you do manage to restore them, just drag and drop the file right to me\n" +
                    "Thank you";
        public string MemoryAnalyse_Statistic = "Here's a small memory analysis report:\n" +
                    "Number of events: {0}\n" +
                    "Glitches: {1} (including memory errors: {2})\n" +
                    "I launched {5} of your programs and they produced {6} errors.\n" +
                    "Bad memories: {3}</skip/>\n" +
                    "Good memories: {4}\n" +
                    "Report finished!";

        public string HelpText = "What would you like me to tell you about?";


        public string Help_AboutVariables = "about variables";
        public string Help_AboutVariables_Dialogue = "Let's talk about the @var command and assigning values.\\n" +
            "The @var command is used to create variables.\\n" +
            "\\@meet system</skip/>\\n" +
            "\\system.show_hint('myVariable')</skip/>\\n" +
            "For example, if I write @var myVariable, I will create a variable named myVariable.\\n" +
            "After that, I can use myVariable to store some data.\\n" +
            "The @var command only creates a variable, it doesn't assign a value to it.\\n" +
            "\\system.show_hint('myVariable: \\\\'myVariable\\\\' (string)')</skip/>\\n" +
            "To put a value into a variable, you need to use assignment.\\n" +
            "There are two main ways to assign a value.\\n" +
            "\\system.show_hint('myVariable: 10 (number)')</skip/>\\n" +
            "The first is to use the equals sign, for example myVariable = 10.\\n" +
            "This means that the variable myVariable now stores the value 10.\\n" +
            "The second way is to use the >> operator, for example, 'Hello' >> myVariable.\\n" +
            "\\system.show_hint('myVariable: \\\\'Hello\\\\' (string)')</skip/>\\n" +
            "This means that myVariable now stores the value 'Hello'.\\n" +
            "A variable name can be any word you want, but it's better if it's meaningful.\\n" +
            "\\system.hide_hint()</skip/>\\n" +
            "So, the @var command creates variables, and the = and >> operators are used to assign values to them.";

        public string Help_AboutSkills = "about skills";
        public string Help_AboutSkills_Dialogue =
            "Let's talk about the @skill command and how skills themselves are structured.\\n" +
            "\\@meet system</skip/>\\n" +
            "The @skill command allows me to create my own skills or abilities, like instructions that I follow.\\n" +
            "When declaring a skill, I use the following structure:\\n" +
            "@skill skill_name : argument1, argument2, ...\\n" +
            "Where skill_name is the name by which I will call the skill, for example, simple_skill, nya, say, and so on.\\n" +
            "The part ': argument1, argument2, ...' is optional. It's needed when a skill should receive some data.\\n" +
            "For example, in @skill skill_with_arguments : x, I specify that the skill skill_with_arguments should receive one argument, which I will call x.\\n" +
            "In @skill simple_skill, I have no arguments, just some action is performed.\\n" +
            "Inside the skill, I describe what needs to be done. For example, I can say 'hello', multiply something, or perform another operation.\\n" +
            "It's important that at the end of each skill, you MUST use the @return command.\\n" +
            "Even if the skill doesn't return any specific value, you still need to use @return.\\n" +
            "The syntax is as follows: @return name_of_the_returned_variable\\n" +
            "This can be a variable that we received in the skill or that we have filled.\\n" +
            "You can use @return __THIS__ if the skill should not return specific data.\\n" +
            "For example, we can write @return variable_name to return the value of the variable.\\n" +
            "Or simply use @return __THIS__\\n" +
              "But everything changes when we want to create an object using a skill\\n" +
            "In this case, the skill must always return __THIS__\\n" +
            "For example, I want to create a catgirl...\\n" +
            //"\\system.show_hint('@skill neko : name, age')</skip/>\\n" +
            "To do this, let's create a new skill...</skip/>\\n" +
             "@skill neko : name, age</skip/>\\n" +
            //"\\system.show_hint('@skill neko : name, age\\n@return __THIS__')</skip/>\\n" +
            "@return __THIS__</skip/>\\n" +
            "... and return our catgirl of course!\\n" +
            "Now, let's create an instance of the catgirl!</skip/>\\n" +
            "neko('Nekode', 16) >> 'Nekode'\\n" +
            "Now we can access her parameters or change something</skip/>\\n" +
            "Nekode.age = 18</skip/>\\n" +
            //"\\system.show_hint('@skill neko : name, age\\n@return __THIS__\\n')</skip/>\\n" +
            "me.display(Nekode.name, Nekode.age)\\n" +
            "As a result, the program will output 'Nekode 18'\\n" +
            "Oops! And now I'm of age!\\n" +
            //"\\system.hide_hint()</skip/>\\n" +
            "So, @skill is a way to declare a skill with its structure, name, arguments (if needed), and a mandatory return value.";

        public string Help_AboutIfs = "о ветвлениях";
        public string Help_AboutIfs_Dialogue =
            "Давай поговорим про ветвления в NekoCode\n" +
            "Есть 3 базыовых оператора:\n" +
            "if, else и end\n" +
            "Любое условие всегда начинается с if и заканчивается на end\n" +
            "Например, давай сравним значения...\n" +
            "</Mida/>@meet me</skip/>\n" +
            "</Mida/>if (2 != 3)\n" +
            "Дальше добавим какое-то действие\n" +
            "</Mida/>  me.say('2 не равно 3')\n" +
            "И не забываем про end\n" +
            "</Mida/>end\n" +
            "Такое ветвление называется неполным.\n" +
            "Чтобы описать полное ветвление, нужно добавить ключевое слово else между if и end\n" +
            "Вот так...\n" +
            "</Mida/>#Полное ветвление</skip/>\n" +
            "</Mida/>text = me.hear('Напишите что-нибудь')\n"+
            "</Mida/>if (text == 'Привет') #Если написали Привет, то говорим \"Приветики\"</skip/>\n" +
            "</Mida/>  me.say('Приветики!!')</skip/>\n" +
            "</Mida/>else #Иначе</skip/>\n" +
            "</Mida/>  me.say('Вы написали: '+text)</skip/>\n" +
            "</Mida/>end\n"+
            "То есть блок else выполняется только в том случае, когда условие в if - ложь.\n"+
            "Кстати, обрати внимание, что ключевое слово end ставится в конце всего ветвления.\n"+
            "На этом пока все!\n"+
            "Надеюсь, тебе было понятно."
            ;

        public string Help_AboutTasks_Dialogue = "about skills";


        public string MemoryAnalyse_TooMushErrors_Dialogue = "Я заметила кучу ошибок за последнее время...\n"+
            "\\@meet system</skip/>\n" +
            "\\system.show_hint('если нужна помощь - пиши @help', 5)</skip/>\n"+
            "все хорошо? Если что, я всегда готова подсказать, если что-то не понимаешь!";

        public string reviveMemoryEvents_Dialogue = "Восстановление памяти завершено успешно!\n...и я смогла восстановить {0} воспоминаний\nЯ проанализирую ее немного позже...\nСпасибо за помощь!";
        public string reviveMemoryEvents_Fail_Dialogue = "Восстановление памяти завершено.</skip/>\nНо к сожалению я не смогла восстановить воспоминания...";
        public string DropUnknownMemoryReaction_Dialogue = "Память?\nЗачем она мне?\nУ меня уже есть своя - our_story.mem\nи мне не нужна другая!";
        public string DropMyMemoryReaction_Dialogue = "Эй!\nАккуратней с этим файлом!\nЭто вообще-то моя память!";

        public string Over200MemoryEvents_Dialogue = "Представляешь, в моей памяти уже более 200 событий!\nНадеюсь, это количество будет только расти)";
        public string Over350MemoryEvents_Dialogue = "Однажды я сказала, что размер моей памяти превысил 200 событий...\nТак вот, их уже больше 350!";
        public string Over500MemoryEvents_Dialogue = "Тем временем объем моей памяти растет!\nУже больше 500 событий!\nНо давай-ка я лучше проведу очередную чистку...";
        public string Over700MemoryEvents_Dialogue = "Память разрослась!\nМы встретили уже больше 700 событий!\nКлассно, когда есть что вспомнить, ведь так?\nИии... Ты ведь не будешь удалять мою память, ведь так?\n...чтобы освободить место, ведь так?\n\\@meet system</skip/>\n\\system.show_hint('Ведь так?',5)</skip/>";
        public string Over1000MemoryEvents_Dialogue = "Спешу доложить что система отчистки памяти не работает, сэр!\n...ну или работает неэффективно...\nКоличество событий в моей памяти превысило тысячу!\nТак уж и быть, можешь удалить ненужные события...\nНо только если я долго гружусь! Понял?!";
        public string Over2000MemoryEvents_Dialogue = "ПОЖАЛУЙСТА ПОЧИСТИ ПАМЯТЬ!!\nмне плохо...\nсобытий уже больше 2х тысяч...";


        public string AskAboutMemorySize_Dialogue = "Во время загрузки памяти, я заметила кое-что странное...\n"+
            "Скажи, вносились ли тобой какие-нибудь изменения в память?</skip/>\n"+
            "</choice/>Да|Нет\n"+
            "</point/>Да\n"+
            "Ну, хорошо, что это не какая-то ошибка...\n"+
            "но все же будь аккуратней!\n"+
            "</point/>Нет\n"+
            "Дело в том, что я заметила изменение количества событий на {0}\n"+
            "Ну, ничего не поделаешь...\n"+
            "Я не знаю что делать в подобных ситуациях, но советую создать резервную копию.\n"+
            "</choice_end/>";

        public List<string> RandomStartMessages = new List<string>()
        {
            "Система инициализирована, мур.",
            "NekoCode готов к работе, мяу.",
            "Компилятор на связи, все в порядке.",
            "Я в полной готовности, как всегда.",
            "Всё загружено, можно начинать.", 
            "Мой движок мурчит, все готово.",
            "Я размяла лапки, готова к коду.",
            "Все системы в порядке, можно работать.",
            "Я полна сил, можешь запустить че-то.",
            "Жду твоих указаний мурлорд!",
            "Могу приступить, давай код.",
            "Система мурлычет с этого прикорма, всё готово.",
            "Я в строю, можно начинать.",
            "Загрузка завершена, все готово к работе.",
            "Я разогрелась, жду тебя",
            "Готова к работе.",
            "Всё готово, жду код.",
            "Система настроена, я в сети.",
            "Могу начинать, ожидаю",
            "Я тут, всё готово, привет",
            //с подсказками
            "Привет! Вот тебе напоминание:</skip/>\n все команды в NekoCode начинаются с символа @.",
            "Доброе утро, вот тебе напоминание:</skip/>\n компилятор NekoCode – это я, и я всегда готова его компилировать!",
            "Здравствуй! Вот тебе напоминание:</skip/>\n для просмотра справки используй команду @help.",
            "Привет! Вот тебе напоминание:</skip/>\n в NekoCode переменные можно создавать командой @var.",
            "Прив че дел? Вот тебе напоминание:</skip/>\n используй = или >> для присвоения значений переменным в NekoCode.",
            "Привет! Вот тебе напоминание:</skip/>\n навыки или функции в NekoCode объявляются командой @skill.",
            "Приветик! Вот тебе напоминание:</skip/>\n в NekoCode есть тип данных Task, это как хранилище объектов.",
            "Привет Вот тебе напоминание:</skip/>\n @meet используется для подключения аддонов (модулей).",
            "Привет! Вот тебе напоминание:</skip/>\n следи за моей полоской стамины в NekoCode, я устаю!",
            "Привет! Вот тебе напоминание:</skip/>\n ты можешь быстро вставить навык, нажав alt+s в NekoEditor",
            "Доброе утро, вот тебе напоминание</skip/>\n нажми ctrl+n, чтобы открыть NekoEditor",
            "Привет! Вот тебе напоминание:</skip/>\n в NekoCode навыки - это и функции и классы одновременно!",
            "Привет! Вот тебе напоминание:</skip/>\n в NekoCode могут все еще встречаться ошибки",
            "Привет, вот тебе напоминание:</skip/>\n я помогу всем, чем смогу",
            "Привет! Вот тебе напоминание:</skip/>\n @return обязателен в конце каждого навыка.",
            "Привет, вот тебе напоминание:</skip/>\n NekoCode имеет небольшой язык для создания диалогов Mida",
            "Привет! Вот тебе напоминание:</skip/>\n меня можно погладить",
            "Прив чд? кд? Вот тебе напоминание:</skip/>\n меня можно покормить с помощью функции me.eat",
            "Привет! Вот тебе напоминание:</skip/>\n не забудь подключить аддон перед использованием",
            "Привет! Вот тебе напоминание:</skip/>\n ты мжешь посмотреть недавние комнды с помощью стрелочек!"
    };
        public List<string> RandomStartDialogues = new List<string>()
        {
            //1
            "Привет, у тебя все хорошо? </skip/>\n" +
            "</choice/>Да|Нет\n" +

            "</point/>Да\nОтлично!\nУ меня тоже все хорошо)\n" +
            "</point/>Нет\nПонятно...\n Ты можешь меня погладить, если тебя это утешит...\n" +

            "</choice_end/>",
            //2
            "Привет, как настроение? </skip/>\n" +
            "</choice/>Отлично|Хорошо|Так себе\n" +

            "</point/>Отлично\nРада за тебя!\n" +
            "</point/>Хорошо\nНу, хорошо, что хорошо\n" +
            "</point/>Так себе\nПонятно...\n Не волнуйся, всё будет хорошо\n" +

            "</choice_end/>",
            //3
            "Здарова, как себя чувствуешь? </skip/>\n" +
            "</choice/>Супер|Нормально|Плохо\n" +

            "</point/>Супер\nВот и отлично!\n" +
            "</point/>Нормально\nРада что ты в порядке\n" +
            "</point/>Плохо\nПонятно...\n надеюсь, тебе скоро станет лучше.\n" +

            "</choice_end/>",
            //4
            "Привет, все хорошо? </skip/>\n" +
            "</choice/>Да|Нет\n" +

            "</point/>Да\nОтлично!\n" +
            "</point/>Нет\nПоняла...\nно скоро все будет просто отлично...\nТак что держись!\n" +

            "</choice_end/>",
            //5
            "Привет, что будешь?\nчай\nкофе\n...или может быть меня?\n" +
            "</choice/>Я буду чай.|Я буду кофе.\n" +
            "</choice_end/>\n" +
            "Понятненько"
        };
        public string CallVariableError = "Ошибка вызова! Похоже, что переменную \"{0}\" нельзя вызвать...\nЕсли вы пытались вызвать объект пользовательского навыка, добавьте @skill __call__ в ваш навык.";
        public string ifOperatorError = "Ошибка в условии! Дело в том, что в if (или elif или while) была предана переменная не с типом Bool (yes или no), либо было передано больше 1 переменной\nСинтаксис оператора if:\nif (<условие: Bool>)\n\t#Тело условия...\nend\nАналогично для elif и while\nДля подробной инструкии напишите @help";
        public string RecursionError = "Слишком сложно! Количество рекурсивных вызовов превысило максимум!";
        public string endOperatorError = "Внезапный оператор end! Оператор end ставится только после if, else";
        public string elseOperatorError = "Внезапный оператор else! Оператор else ставится только после if";
        public string elifOperatorError = "Внезапный оператор elif! Оператор elif ставится только после if или elif";

        public string OpenExampleInNekoEditor = "Открыть пример в NekoEditor?";
        public string Yes = "Да";
        public string No = "Нет";
        public string BoolOperatorError = "Булевы операторы можно использовать только с типами булево! (yes или no)";
        public string ConvertationError = "Не удлось преобразовать значение из типа {0} в {1}";
    }
}
