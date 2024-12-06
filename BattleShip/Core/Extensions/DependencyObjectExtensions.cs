using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace QuickGuide.Core.Extensions
{
    public static class DependencyObjectExtensions
    {
        public static T FindAncestor<T>(this DependencyObject obj) where T : DependencyObject
        {
            if (obj != null)
            {
                var dependObj = obj;
                do
                {
                    dependObj = GetParent(dependObj);
                    if (dependObj is T)
                        return dependObj as T;
                }
                while (dependObj != null);
            }

            return null;
        }
        public static DependencyObject GetParent(this DependencyObject obj)
        {
            if (obj == null)
                return null;
            if (obj is ContentElement)
            {
                var parent = ContentOperations.GetParent(obj as ContentElement);
                if (parent != null)
                    return parent;
                if (obj is FrameworkContentElement)
                    return (obj as FrameworkContentElement).Parent;
                return null;
            }

            return VisualTreeHelper.GetParent(obj);
        }

        public static T FindChild<T>(this DependencyObject parent, string childName) where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }
        public static T FindChild<T>(this DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            T foundChild = null;
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                {
                    return typedChild;
                }

                foundChild = child.FindChild<T>();
                if (foundChild != null) break;
            }

            return foundChild;
        }
        public static FrameworkElement FindChild(this DependencyObject parent, string childName)
        {
            if (parent == null) return null;

            FrameworkElement foundChild = null;
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is FrameworkElement frameworkElement && frameworkElement.Name == childName)
                {
                    return frameworkElement;
                }

                foundChild = child.FindChild(childName);
                if (foundChild != null) break;
            }

            return foundChild;
        }



        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) yield return (T)Enumerable.Empty<T>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject ithChild = VisualTreeHelper.GetChild(parent, i);
                if (ithChild == null) continue;
                if (ithChild is T t) yield return t;
                foreach (T childOfChild in FindVisualChildren<T>(ithChild)) yield return childOfChild;
            }
        }
    }
}
