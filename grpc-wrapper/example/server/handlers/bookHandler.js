export default {
    getBookByID: (call, callback) => {
        const books = [
            {id: "1", title: "Book 1", author: "Author 1"},
            {id: "2", title: "Book 2", author: "Author 2"},
            {id: "3", title: "Book 3", author: "Author 3"}
        ];

        const filtered = books.filter(book => book.id === call.request.id);
        if (!filtered)
            callback({
                code: 5,
                details: `Book with ID ${call.request.id} not found`
            });
        callback(null, {book: filtered[0]});
    }
}
