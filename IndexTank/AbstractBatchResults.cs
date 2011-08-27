using System;
using System.Collections.Generic;
using System.Linq;

namespace IndexTank
{
    public abstract class AbstractBatchResults<T>
    {

        private bool hasErrors;
        private List<bool> results;
        private List<string> errors;
        private List<T> elements;

        public AbstractBatchResults(List<bool> results, List<string> errors, List<T> elements, bool hasErrors)
        {
            this.results = results;
            this.errors = errors;
            this.hasErrors = hasErrors;
            this.elements = elements;
        }

        public bool GetResult(int position)
        {
            if (position >= results.Count())
            {
                throw new ArgumentOutOfRangeException(String.Format("Position off bounds ({0})", position));
            }

            return results.ElementAt(position);
        }

        /**
         * Get the error message for a specific position. Will be null if
         * getResult(position) is false.
         * 
         * @param position
         * @return the error message
         */
        public string GetErrorMessage(int position)
        {
            if (position >= errors.Count())
            {
                throw new ArgumentOutOfRangeException("Position off bounds (" + position + ")");
            }

            return errors.ElementAt(position);
        }

        /**
         * @return <code>true</code> if at least one of the documents failed to
         *         be indexed
         */
        public bool HasErrors()
        {
            return hasErrors;
        }

        protected T GetElement(int position)
        {
            return elements.ElementAt(position);
        }

        /// <summary>
        /// return an iterable with all the {@link Document}s
        /// that couldn't be indexed. It can be used to retrofeed the
        /// AddDocuments method.
        /// </summary>
        /// <returns></returns>                
        protected IEnumerable<T> GetFailedElements()
        {
            return null;
            //return new Iterable<T>() {
            //    @Override
            //    public Iterator<T> iterator() {
            //        return new Iterator<T>() {
            //            private T next = computeNext();
            //            private int position = 0;

            //            private T computeNext() {
            //                while (position < results.size()
            //                        && results.get(position)) {
            //                    position++;
            //                }

            //                if (position == results.size()) {
            //                    return null;
            //                }

            //                T next = elements.get(position);
            //                position++;
            //                return next;
            //            }

            //            @Override
            //            public void remove() {
            //                throw new UnsupportedOperationException();
            //            }

            //            @Override
            //            public T next() {
            //                if (!hasNext()) {
            //                    throw new NoSuchElementException();
            //                }

            //                T result = this.next;
            //                this.next = computeNext();
            //                return result;
            //            }

            //            @Override
            //            public boolean hasNext() {
            //                return next != null;
            //            }
            //        };
        }
    }
}
