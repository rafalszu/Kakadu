export function handleResponse(response) {
    // if(response.status === 401) {
    //     localStorage.removeItem('user');

    //     return Promise.reject(response.statusText)
    // }
    return response.text().then(text => {
        const data = text && JSON.parse(text);
        if (!response.ok) {
            
            const error = { 
                response: (data && data.Message) || response.url + ' - ' + response.statusText
            };
            return Promise.reject(error);
        }

        return data;
    });
};