﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using EpubSharp;
using EpubSharp.Format;
using OnlineLib2Ebook.Extensions;
using OnlineLib2Ebook.Types.Book;

namespace OnlineLib2Ebook.Logic.Builders {
    public class EpubBuilder : BuilderBase {
        private readonly EpubWriter _writer;
        private readonly string _pattern;

        private EpubBuilder(string pattern) {
            _writer = new EpubWriter();
            _pattern = pattern;
        }

        /// <summary>
        /// Создание нового объекта Builder'a
        /// </summary>
        /// <returns></returns>
        public static EpubBuilder Create(string pattern) {
            return new(pattern);
        }

        /// <summary>
        /// Создание Xhtml документа из кода части
        /// </summary>
        /// <param name="title">Заголовок части</param>
        /// <param name="decodeText">Раскодированный текст</param>
        /// <returns></returns>
        private string ApplyPattern(string title, string decodeText) {
            return _pattern.Replace("{title}", title).Replace("{body}", decodeText).AsXHtmlDoc().AsString();
        }

        /// <summary>
        /// Добавление автора книги
        /// </summary>
        /// <param name="author">Автор</param>
        /// <returns></returns>
        public override BuilderBase AddAuthor(string author) {
            _writer.AddAuthor(author);
            return this;
        }

        /// <summary>
        /// Указание названия книги
        /// </summary>
        /// <param name="title">Название книги</param>
        /// <returns></returns>
        public override BuilderBase WithTitle(string title) {
            _writer.SetTitle(title);
            return this;
        }

        /// <summary>
        /// Добавление обложки книги
        /// </summary>
        /// <param name="cover">Обложка</param>
        /// <returns></returns>
        public override BuilderBase WithCover(Image cover) {
            if (cover != null) {
                _writer.SetCover(cover.Content, cover.Format);
            }

            return this;
        }

        /// <summary>
        /// Добавление внешних файлов
        /// </summary>
        /// <param name="directory">Путь к директории с файлами</param>
        /// <param name="searchPattern">Шаблон поиска файлов</param>
        /// <param name="type">Тип файла</param>
        /// <returns></returns>
        public override BuilderBase WithFiles(string directory, string searchPattern, EpubContentType type) {
            foreach (var file in Directory.GetFiles(directory, searchPattern)) {
                Console.WriteLine($"Добавляем файл {file.CoverQuotes()}");
                _writer.AddFile(Path.GetFileName(file), File.ReadAllBytes(file), type);
            }

            return this;
        }

        /// <summary>
        /// Добавление списка частей книги
        /// </summary>
        /// <param name="chapters">Список частей</param>
        /// <returns></returns>
        public override BuilderBase WithChapters(IEnumerable<Chapter> chapters) {
            foreach (var chapter in chapters.Where(c => c.IsValid)) {
                foreach (var image in chapter.Images) {
                    _writer.AddFile(image.Path, image.Content, image.Format.ToEpubContentType());
                }

                Console.WriteLine($"Добавляем часть {chapter.Title.CoverQuotes()}");
                _writer.AddChapter(HttpUtility.HtmlDecode(chapter.Title), ApplyPattern(chapter.Title, chapter.Content));
            }

            return this;
        }

        protected override void BuildInternal(string name) {
            _writer.Write(name);
        }

        protected override string GetFileName(string name) {
            return $"{name}.epub".RemoveInvalidChars();
        }
    }
}