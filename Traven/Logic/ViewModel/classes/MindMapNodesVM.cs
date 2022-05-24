using Traven.Logic.ViewModel.classes;
using Traven.Logic.ViewModel.interfaces;
using Traven.Repository;
using Traven.UOW;
using Traven.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Traven.Logic.ViewModel.classes
{
    using EDuplicateNode = Exception;
    using ENodeNotExist = Exception;
    using ENodeNotDeleted = Exception;
    using EInvalidTreeElement = Exception;
    using EInvalidMainNode = Exception;
    using EMindMapNotEmpty = Exception;
    using ENodeIsNull = Exception;
    using ETreeDeleteNotAllowed = Exception;
    using System.Windows.Controls;
    using System.Windows.Shapes;
    using System.Windows;
    using System.Xml.Linq;
    class MindMapNodesVM : AMindMapVM
    {
        protected class TreeElement
        {
            private AMindMapVM _mindmap = null;
            public TreeElement parent = null;
            public ANodeVM node = null;
            public List<TreeElement> children = null;
            public TreeElement(AMindMapVM mindmap)
            {
                this._mindmap = mindmap;
                this.children = new List<TreeElement>();
            }

            public TreeElement getAnyChild(ANodeVM node)
            {
                if (this.node == null)
                    throw new EInvalidTreeElement("Узел не был установлен в этом элементе дерева!");

                foreach (TreeElement child in children)
                {
                    TreeElement result = null;
                    if (child.node == node)
                    {
                        return child;
                    }
                    else if ((result = child.getAnyChild(node)) != null)
                    {
                        return result;
                    }
                }
                return null;
            }

            public void addChild(ANodeVM node)
            {
                TreeElement element = new TreeElement(this._mindmap);
                element.node = node;
                element.parent = this;
                this.children.Add(element);
            }

            internal void delete(TreeElement knoten, bool recursive)
            {
                if ((knoten.children.Count > 0) && !recursive)
                    throw new ETreeDeleteNotAllowed("Узел, который вы хотели удалить, все еще имеет дочерние узлы.");

                if (recursive)
                {
                    for (int count = knoten.children.Count - 1; count >= 0; count--)
                    {
                        TreeElement child = knoten.children[count];
                        this._mindmap.removeNode(child.node, true);
                    }
                }
                this.children.Remove(knoten);
            }

            internal void invalidateChilds()
            {
                foreach (TreeElement child in children)
                    child.node.invalidate();
            }

            internal XElement toXML()
            {
                XElement node = this.node.toXML();
                XElement childs = new XElement("childs");
                foreach (TreeElement child in children)
                {
                    childs.Add(child.toXML());
                }
                node.Add(childs);
                return node;
            }


        }

        private TreeElement _nodeRegistry;

        protected TreeElement getNode(ANodeVM node)
        {
            if (node == null)
                throw new ENodeIsNull("Переданный узел равен нулю!");
            return this._nodeRegistry.getAnyChild(node);
        }

        public MindMapNodesVM()
        {
            this._nodeRegistry = new TreeElement(this);
        }

        public override void setMainNode(ANodeVM node, bool ignoreNotEmpty = false)
        {
            if (node == null)
                throw new ENodeIsNull("Переданный узел равен нулю!");
            if (node.getParent() != null)
                throw new EInvalidMainNode("Установлен родитель, который не разрешен в основном узле.");
            if ((this._nodeRegistry.node != null) && (!ignoreNotEmpty))
                throw new EMindMapNotEmpty("Интеллект карта пуста!");
            this._nodeRegistry.node = node;
            node.changeNodeEvent += node_changeNodeEvent;

            this.onAddNode(this, node);
        }

        private Size _drawSize;

        private void node_changeNodeEvent(object sender, ANodeVM node)
        {
            if (this._nodeRegistry.node == node)
                this._nodeRegistry.invalidateChilds();
            else
            {
                TreeElement element = this._nodeRegistry.getAnyChild(node);
                if (element != null)
                    element.invalidateChilds();
            }

            this.setDrawSize(new Size(Math.Max(this._drawSize.Width, node.getRectangle().Right),
                Math.Max(this._drawSize.Height, node.getRectangle().Bottom)));
        }

        public override void addNode(ANodeVM element)
        {
            if (element == null)
                throw new ENodeIsNull("Переданный узел равен нулю!");
            if ((this._nodeRegistry.getAnyChild(element) != null) && (this._nodeRegistry.node != element))
                throw new EDuplicateNode("Узел, который вы хотели добавить, уже существует!");
            TreeElement parent = this._nodeRegistry.getAnyChild(element.getParent());
            if ((parent == null) && (this._nodeRegistry.node != element.getParent()))
                throw new ENodeNotExist("Родительский узел не существует в дереве.");

            if (parent == null)
                parent = this._nodeRegistry;

            parent.addChild(element);
            element.changeNodeEvent += node_changeNodeEvent;

            this.onAddNode(this, element);
        }

        public override void removeNode(ANodeVM element, bool recursive = false)
        {
            if (element == null)
                throw new ENodeIsNull("Переданный узел равен нулю!");
            TreeElement knoten = this._nodeRegistry.getAnyChild(element);
            if ((knoten == null) || (this._nodeRegistry.node == element))
                throw new ENodeNotExist("Узел, который вы хотели удалить, не существует или является основным узлом.");

            TreeElement parent = knoten.parent;
            parent.delete(knoten, recursive);

            this.onRemoveNode(this, element);
        }

        public override ANodeVM getMainNode()
        {
            return this._nodeRegistry.node;
        }

        public override XElement toXML()
        {
            XElement blub = new XElement("mindmap");
            blub.Add(this._nodeRegistry.toXML());
            return blub;
        }

        public override void fromXML(XElement XML)
        {
            this._nodeRegistry = new TreeElement(this);
            XElement blub = XML;
            ANodeVM start = new NodeVM();
            start.fromXML(blub.Element("node"));
            this.setMainNode(start);
            addChildXMLNode(blub.Element("node"), start);
        }

        private void addChildXMLNode(XElement nodeXML, ANodeVM parent)
        {
            foreach (XElement child in nodeXML.Element("childs").Elements("node"))
            {
                ANodeVM node = new NodeVM();
                node.fromXML(child);
                node.setParent(parent);
                this.addNode(node);
                addChildXMLNode(child, node);
            }
        }

        public override void setDrawSize(Size newSize)
        {
            this._drawSize.Width = Math.Max(newSize.Width, this._drawSize.Width);
            this._drawSize.Height = Math.Max(newSize.Height, this._drawSize.Height);
            this.changeDrawSize(this, this._drawSize);
        }

        public override Size getDrawSize()
        {
            return this._drawSize;
        }
    }
}
