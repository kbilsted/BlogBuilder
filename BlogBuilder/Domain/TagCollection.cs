using System;
using System.Collections;
using System.Collections.Generic;

namespace Kbg.BlogBuilder.Domain
{
    public class TagCollection : IEnumerable<KeyValuePair<Tag, List<Page>>>
    {
        readonly Dictionary<string, Tag> lowerCaseDistinct = new Dictionary<string, Tag>();
        readonly Dictionary<Tag, List<Page>> Tags = new Dictionary<Tag, List<Page>>();

        public TagCollection(IEnumerable<Tuple<Tag, Page[]>> tags)
        {
            foreach (var tagInfo in tags)
                Add(tagInfo.Item1, tagInfo.Item2);
        }

        public TagCollection(IEnumerable<TagCollection> tagCollections)
        {
            foreach (var collection in tagCollections)
                Add(collection);
        }

        public IEnumerator<KeyValuePair<Tag, List<Page>>> GetEnumerator()
        {
            return Tags.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(Tag tag, params Page[] url)
        {
            if (!lowerCaseDistinct.TryGetValue(tag.Value.ToLower(), out var distinctTag))
            {
                distinctTag = tag;
                lowerCaseDistinct.Add(tag.Value.ToLower(), tag);
            }

            if (!Tags.TryGetValue(distinctTag, out var urls))
                Tags.Add(tag, urls = new List<Page>());
            urls.AddRange(url);
        }

        public void Add(TagCollection tagsForPage)
        {
            foreach (var kv in tagsForPage.Tags)
                Add(kv.Key, kv.Value.ToArray());
        }
    }
}