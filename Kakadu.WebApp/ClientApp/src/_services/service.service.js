import { authHeader, apiUrl } from '../_helpers';

export const serviceService = {
    getAll
};

function getAll() {
    const requestOptions = {
        method: 'GET',
        headers: authHeader()
    };

    return fetch(`${apiUrl}/service`, requestOptions).then(handleResponse);
}

function handleResponse(response) {
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
}