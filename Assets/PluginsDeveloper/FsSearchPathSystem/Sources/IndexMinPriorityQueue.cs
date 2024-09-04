using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 最小索引优先队列
/// </summary>
/// <typeparam name="T"></typeparam>
public class IndexMinPriorityQueue<T> where T : IComparable<T>
{
    /// <summary>
    /// 存储元素的数组
    /// </summary>
    private T[] items;

    /// <summary>
    /// 保存每个元素在items数组中的索引，pq数组是真正的堆，需要堆有序
    /// </summary>
    private int[] pq;

    /// <summary>
    /// 保存qp的逆序，pq的值作为索引，pq的索引作为值
    /// </summary>
    private int[] qp;

    /// <summary>
    /// 记录堆中元素的个数
    /// </summary>
    private int N;

    public IndexMinPriorityQueue(int capacity)
    {
        items = new T[capacity + 1];
        pq = new int[capacity + 1];
        qp = new int[capacity + 1];
        N = 0;

        //默认情况下，items中没有数据，则qp中值全为-1,-1代表没有数据与此索引关联。
        for (int i = 0; i < qp.Length; i++)
        {
            qp[i] = -1;
        }
    }

    /// <summary>
    /// 获取队列中元素的个数
    /// </summary>
    /// <returns></returns>
    public int size()
    {
        return N;
    }

    /// <summary>
    /// 判断队列是否为空
    /// </summary>
    /// <returns></returns>
    public bool isEmpty()
    {
        return N == 0;
    }

    /// <summary>
    /// 判断堆中索引i处的元素是否小于索引j处的元素
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    /// <returns></returns>
    private bool less(int i, int j)
    {
        return items[pq[i]].CompareTo(items[pq[j]]) < 0;
    }

    /// <summary>
    /// 交换堆中i索引和j索引处的值
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    private void exch(int i, int j)
    {
        //交互pq中数据
        int temp = pq[i];
        pq[i] = pq[j];
        pq[j] = temp;

        //更新qp中的数据
        qp[pq[i]] = i;
        qp[pq[j]] = j;
    }

    /// <summary>
    /// 判断k对应的元素是否存在
    /// </summary>
    /// <param name="k"></param>
    /// <returns></returns>
    public bool contains(int k)
    {
        return qp[k] != -1;
    }

    /// <summary>
    /// 最小元素关联的索引
    /// </summary>
    /// <returns></returns>
    public int minIndex()
    {
        return pq[1];
    }

    /// <summary>
    /// 往队列中插入一个元素,并关联索引i
    /// </summary>
    /// <param name="i"></param>
    /// <param name="t"></param>
    public void insert(int i, T t)
    {
        //判断i是否已近被关联，如果已经被关联，则不让插入
        if (contains(i))
        {
            return;
        }
        //元素个数+1
        N++;
        //把数据存入到items对应的i位置上
        items[i] = t;
        //把i存储到pq中
        pq[N] = i;
        //通过qp来记录pq中的i
        qp[i] = N;
        //通过上浮完成pq数组堆的调整
        swim(N);
    }

    /// <summary>
    /// 删除队列中最小的元素,并返回该元素关联的索引
    /// </summary>
    /// <returns></returns>
    public int delMin()
    {
        //获取最小元素的索引
        int minIndex = pq[1];
        //交互pq中最小索引1处和最大索引N处的值
        exch(1, N);
        //删除items数组中对应索引（pq中最大索引N处的值）中的值
        items[pq[N]] = default(T);
        //删除qp中对应索引（pq中最大索引N处的值）处的值
        qp[pq[N]] = -1;
        //删除pq中最大索引N处的值
        pq[N] = -1;
        //元素个数-1
        N--;
        //通过下沉算法调整堆，使得堆有序
        sink(1);

        return minIndex;
    }

    /// <summary>
    /// 删除索引i关联的元素
    /// </summary>
    /// <param name="i"></param>
    public void delete(int i)
    {
        //找到items中i索引在pq中的索引k
        int k = qp[i];
        //交换pq中k索引和N索引处的值
        exch(k, N);
        //删除items中i位置的值
        items[pq[N]] = default(T);
        //删除qp中对应位置的值
        qp[pq[N]] = -1;
        //删除pq中最大索引位置的值
        pq[N] = -1;
        //元素个数-1
        N--;
        //因为索引k不一定是索引1，因此需要同时使用上浮和下沉算法调整堆，使其有序
        swim(k);
        sink(k);
    }

    /// <summary>
    /// 把与索引i关联的元素修改为为t
    /// </summary>
    /// <param name="i"></param>
    /// <param name="t"></param>
    public void changeItem(int i, T t)
    {
        //修改items中i索引处的值
        items[i] = t;
        //找到i在pq中索引
        int k = qp[i];
        //调整堆
        swim(k);
        sink(k);
    }

    /// <summary>
    /// 使用上浮算法，使索引k处的元素能在堆中处于一个正确的位置
    /// </summary>
    /// <param name="k"></param>
    private void swim(int k)
    {
        //循环判断当前节点是否比起父结点小，若是，则交换，直至循环到父结点即索引为1，停止循环
        while (k > 1)
        {
            if (less(k, k / 2))
            {
                exch(k, k / 2);
            }
            k = k / 2;
        }
    }

    /// <summary>
    /// 使用下沉算法，使索引k处的元素能在堆中处于一个正确的位置
    /// </summary>
    /// <param name="k"></param>
    private void sink(int k)
    {
        //循环比较当前节点是否小于子结点中较小值，若是，则交换，直至循环到当前节点没有子结点，即左子结点为null
        while (k * 2 <= N)
        {
            int minIndex;
            if (2 * k + 1 <= N)
            {
                if (less(2 * k, 2 * k + 1))
                    minIndex = 2 * k;
                else
                    minIndex = 2 * k + 1;
            }
            else
            {
                minIndex = 2 * k;
            }
            if (less(k, minIndex))
            {
                break;
            }
            exch(k, minIndex);
            k = minIndex;
        }
    }
}
