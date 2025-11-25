using Commander.Communication;
using Commander.Executor;
using Commander.Terminal;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Spectre.Console;
using Shared;

namespace Commander.Commands.Agent
{
    public class MapCommandOptions
    {
    }

    public class MapCommand : EnhancedCommand<MapCommandOptions>
    {
        public override string Category => CommandCategory.Commander;
        public override string Description => "Show current agents graph";
        public override string Name => "map";

        public override ExecutorMode AvaliableIn => ExecutorMode.All;

        public override RootCommand Command => new RootCommand(this.Description);

        protected async override Task<bool> HandleCommand(CommandContext<MapCommandOptions> context)
        {

            var tree = this.GenerateTree(context);
            this.RenderTree(tree, context);


            return true;
        }

        private void RenderTree(MapTreeNode root, CommandContext<MapCommandOptions> context)
        {
            //this.RenderNode(root, context);
            var guiTree = new Tree(new Markup("[" + (root.IsAlive ? "green" : "grey") + "]" + root.Name + "[/]")).Guide(TreeGuide.Line);
            guiTree.Style = new Style(Spectre.Console.Color.Olive);

            foreach (var child in root.Children)
            {
                guiTree.AddNode(RenderNode(child));
            }

            context.Terminal.Write(guiTree);


        }

        private TreeNode AsGuiNode(MapTreeNode node)
        {
            return new TreeNode(new Markup("[cyan]" + node.LinkToParent + "[/] <=> [" + (node.IsAlive ? "green" : "grey") + "]" + node.Name + " - " + node.Id +  "[/]"));
        }

        private TreeNode RenderNode(MapTreeNode node)
        {
            var guiNode = AsGuiNode(node);
            foreach (var child in node.Children)
                guiNode.AddNode(RenderNode(child));

            return guiNode;
        }

        private MapTreeNode GenerateTree(CommandContext<MapCommandOptions> context)
        {
            var allAgents = context.CommModule.GetAgents();

            MapTreeNode root = new MapTreeNode() { Name = "TeamServer", IsAlive = context.CommModule.ConnectionStatus == ConnectionStatus.Connected, ShortId = string.Empty };
            Dictionary<string, MapTreeNode> allNodes = new Dictionary<string, MapTreeNode>();
            foreach (var agent in allAgents)
            {
                var node = new MapTreeNode();
                node.Name = agent.Metadata?.Desc;
                node.Id = agent.Id;
                node.ShortId = agent.Id;
                node.IsAlive = context.IsAgentAlive(agent) == true;
                allNodes.Add(agent.Id, node);
            }

            foreach (var agent in allAgents)
            {
                var node = allNodes[agent.Id];
                var conn = ConnexionUrl.FromString(agent.Metadata.EndPoint);
                node.LinkToParent = conn.ProtocolString;

                Models.Agent parent = null;
                foreach(var potParent in allAgents.Where(a => a.Id != agent.Id))
                {
                    if (potParent.Links.Any(childId => childId == agent.Id))
                    {
                        parent = potParent;
                        break;
                    }
                }

                if(parent == null)
                    root.Children.Add(node);
                else
                    allNodes[parent.Id].Children.Add(node);
            }

            return root;
        }
    }


    //public enum MapLinkType
    //{
    //    Unknown,
    //    Http,
    //    Pipe
    //}

    public class MapTreeNode
    {
        public string Id { get; set; }

        public string ShortId { get; set; }
        public string Name { get; set; }
        public List<MapTreeNode> Children { get; set; } = new List<MapTreeNode>();

        public string LinkToParent { get; set; } = "Unknown";

        public bool IsAlive { get; set; }
    }


}
