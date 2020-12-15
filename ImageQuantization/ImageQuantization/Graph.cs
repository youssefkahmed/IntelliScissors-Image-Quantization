using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace ImageQuantization
{
    struct Edge : IComparable<Edge>
    {
        // Each Edge Consists Of A Source, Destination, And Weight
        public RGBPixel src, dest;
        public float weight;

        // Constructor
        public Edge(RGBPixel src, RGBPixel dest, float weight)
        {
            this.src = src;
            this.dest = dest;
            this.weight = weight;
        }

        // In The MST, Edges Have To Be Sorted Ascendingly
        // According To Their Weights, So This
        // Function Is Used To Base The Sorting Operation
        // On Weights
        public int CompareTo(Edge compareEdge)
        {
            if (this.weight > compareEdge.weight)
            {
                return 1;
            }

            else if (this.weight < compareEdge.weight)
            {
                return -1;
            }

            else
                return 0;
        }
    }

    // This Class Represents Each Node In The Priority Queue
    // Each Node Contains A Vertex (Color) And A Pointer
    // To That Node's Parent Vertex
    class PQNode : FastPriorityQueueNode
    {
        public RGBPixel currentVertex;
        public RGBPixel? Parent;
        public PQNode(RGBPixel vertex, RGBPixel? parent)
        {
            currentVertex = vertex;
            Parent = parent;
        }


    }


    class Graph
    {
        // Member Variables
        int vertex;
        public List<Edge> MST;
        public float MSTSum;
        static Dictionary<RGBPixel, List<RGBPixel>> neighbours;

        // Constructor
        public Graph(int v)
        {
            this.vertex = v; ;
            MST = new List<Edge>();
            neighbours = new Dictionary<RGBPixel, List<RGBPixel>>();
            MSTSum = 0;
        }


        // This Region Contains All The Necessary Functions
        // To Implement Prim's Algorithm For
        // The Minimum Spanning Tree
        #region Spanning Tree Methods

        // Used To Calculate The Weight Difference Between Two Nodes
        private static float WeightDifference(PQNode N1, PQNode N2)
        {
            byte red1, green1, blue1;
            byte red2, green2, blue2;

            // Assigning The RGB Values Of Each Color
            red1 = N1.currentVertex.red;
            red2 = N2.currentVertex.red;
            green1 = N1.currentVertex.green;
            green2 = N2.currentVertex.green;
            blue1 = N1.currentVertex.blue;
            blue2 = N2.currentVertex.blue;

            // Calculating The Difference Between Them
            return (float)Math.Sqrt((red2 - red1) * (red2 - red1) + (green2 - green1) * (green2 - green1) + (blue2 - blue1) * (blue2 - blue1));
        }

        // The Function That Calculates The Minimum Spanning Tree For The Graph
        // Its Only Parameter Is The List Of Distinct Colors
        public void MinSpanningTree(List<RGBPixel> LoD)
        {
            // Using A Priority Queue So That The Edges Are
            // Sorted According To Their Weights In Ascending Order
            // The Priority Queue Contains The Number Of Vertices (LoD.Count)
            FastPriorityQueue<PQNode> PQ = new FastPriorityQueue<PQNode>(LoD.Count);

            // An Array That Contains Each Node (color)
            // And A Pointer To That Node's Parent
            PQNode[] PQNode = new PQNode[LoD.Count];

            // Initialising The First Node In The Array
            // Its Parent Is Null
            PQNode[0] = new PQNode(LoD[0], null);
            // Inserting That First Node Into The Priority Queue
            // With A Value of 0
            PQ.Enqueue(PQNode[0], 0);

            float weight;

            // For Each Vertex (Minus The Already Added One), 
            // Insert That Vertex Into The Array
            // With Parent = null, Then Insert It Into The PQ
            // With Values = Infinity
            for (int i = 1; i < LoD.Count; i++)
            {
                PQNode[i] = new PQNode(LoD[i], null);
                PQ.Enqueue(PQNode[i], int.MaxValue);
            }

            // Looping Until The Priority Queue Is Empty
            while (PQ.Count != 0)
            {
                // Dequeuing The First (Minimum) Element
                // From The PQ
                PQNode Minimum = PQ.Dequeue();

                // Checking If The Minimum Element Is The Root Node Or Not
                // (Only The Root Node Has A Null Parent In The First Iteration)
                if (Minimum.Parent != null)
                {
                    Edge edge;
                    edge.src = Minimum.currentVertex;
                    edge.dest = (RGBPixel)Minimum.Parent;
                    edge.weight = (Minimum.currentWeight);

                    MST.Add(edge);     // Add the minimum weight to the MST.
                    MSTSum += edge.weight; // Add The Edge's Weight To The MST Sum
                }

                // We Have To Modify The Values Of The PQ
                // Each Time
                foreach (var node in PQ)
                {
                    // Calculating The Weight Difference Between The Current Node
                    // And The Minimum Node
                    weight = WeightDifference(node, Minimum);

                    // If That Weight Difference Is Less Than The Node's Current Weight
                    // Then The Minimum Node Becomes The Parent Of That Node
                    // And We Adjust That Node's Weight To The Weight Difference
                    // By Updating The PQ
                    if (weight < node.currentWeight)
                    {
                        node.Parent = Minimum.currentVertex;
                        PQ.UpdatePriority(node, weight);
                    }
                }
            }

        }

        #endregion

        #region Clustering Methods
        private static void DFS(RGBPixel currentColor, ref HashSet<RGBPixel> visited, ref List<RGBPixel>cluster)
        {
            visited.Add(currentColor);
            cluster.Add(currentColor);
            foreach (var neighbour in neighbours[currentColor])
            {
                if (!visited.Contains(neighbour))
                    DFS(neighbour, ref visited, ref cluster);
            }
        }
        public static List<List<RGBPixel>> Cluster(List<Edge> MST, int k)
        {
            List<List<RGBPixel>> clusters = new List<List<RGBPixel>>();
            HashSet<RGBPixel> visited = new HashSet<RGBPixel>();
            float MaxWeight;
            int MaxIndex;
            
            for (int j = 0; j < k - 1; j++)
            {
                MaxWeight = 0;
                MaxIndex = 0;

                for (int i = 0; i < MST.Count; i++)
                {
                    if (MST[i].weight > MaxWeight)
                    {
                        MaxWeight = MST[i].weight;
                        MaxIndex = i;
                    }
                }

                Edge e= new Edge(MST[MaxIndex].src, MST[MaxIndex].dest,0);
                MST[MaxIndex] = e;
            }

            

            foreach (var edge in MST)
            {
                if (edge.weight != 0)
                {
                    if (neighbours.ContainsKey(edge.src))
                    {
                        neighbours[edge.src].Add(edge.dest);
                        if (!neighbours.ContainsKey(edge.dest)) {
                            neighbours.Add(edge.dest,new List<RGBPixel> {edge.src});
                        }
                    }
                    else if(neighbours.ContainsKey(edge.dest))
                    {
                        neighbours[edge.dest].Add(edge.src);
                        if (!neighbours.ContainsKey(edge.src))
                        {
                            neighbours.Add(edge.src, new List<RGBPixel> { edge.dest});
                        }
                    }
                   
                    else
                    {
                      
                        neighbours.Add(edge.src,new List<RGBPixel> {edge.dest});
                        neighbours.Add(edge.dest, new List<RGBPixel> { edge.src });
                    }
                }
                else
                {
                    if (!neighbours.ContainsKey(edge.src))
                    {
                        List<RGBPixel> l = new List<RGBPixel>();
                        neighbours.Add(edge.src, l);
                    }
                    if (!neighbours.ContainsKey(edge.dest))
                    {
                        List<RGBPixel> l = new List<RGBPixel>();
                        neighbours.Add(edge.dest, l);
                    }
                }
            }

            



            foreach (var vertex in neighbours)
            {
                if (!visited.Contains(vertex.Key))
                {
                    List<RGBPixel> clusterColors = new List<RGBPixel>();
                    DFS(vertex.Key, ref visited , ref clusterColors);
                    clusters.Add(clusterColors);
                  
                }
            }

            return clusters;
        }

        #endregion

        #region Extracting Representative Colors

        // Function Used To Calculate The Average Color For Each Cluster
        public static List<RGBPixel> ExtractColors(List<List<RGBPixel>> clusters)
        {
            // List of colors to be returned
            List<RGBPixel> Palette = new List<RGBPixel>();
            // The Average Color Of Each Cluster
            RGBPixel avgColor;
            // Accumulators For The Sum Of RGB Values Inside Each Cluster
            int redSum, greenSum, blueSum;

            for (int i = 0; i < clusters.Count; i++)
            {
                redSum = 0; greenSum = 0; blueSum = 0;

                // For Every Color In Cluster i, Add Up All Their
                // RGB Values
                foreach (var color in clusters[i])
                {
                    redSum += color.red;
                    greenSum += color.green;
                    blueSum += color.blue;
                }

                // Calculate The Average Color
                avgColor.red = (byte)(redSum / clusters[i].Count);
                avgColor.green = (byte)(greenSum / clusters[i].Count);
                avgColor.blue = (byte)(blueSum / clusters[i].Count);

                // Add The Average Color To The Palette
                Palette.Add(avgColor);
            }

            return Palette;
        }

        #endregion

        #region Quantizing The Image

        public static RGBPixel[,] QuantizedImage(RGBPixel[,] imageMatrix, List<RGBPixel> palette)
        {
            int height = imageMatrix.GetLength(0);
            int width = imageMatrix.GetLength(1);

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    imageMatrix[i, j] = calculateWeight(palette, imageMatrix[i, j]);
                }
            }


            return imageMatrix;
        }
        public static RGBPixel calculateWeight(List<RGBPixel> palette, RGBPixel myCurrentPixel)
        {
            RGBPixel returnPixel = myCurrentPixel;
            double weight = 10000000;
            int r1, g1, b1;
            int r2, g2, b2;

            for (int i = 0; i < palette.Count; i++)
            {
                r1 = palette[i].red;
                g1 = palette[i].green;
                b1 = palette[i].blue;
                r2 = myCurrentPixel.red;
                g2 = myCurrentPixel.green;
                b2 = myCurrentPixel.blue;
                double eq = Math.Sqrt((r2 - r1) * (r2 - r1) + (g2 - g1) * (g2 - g1) + (b2 - b1) * (b2 - b1));

                if (eq < weight)
                {
                    returnPixel = palette[i];
                    weight = eq;
                }
            }
            return returnPixel;
        }

        #endregion
    }
}
