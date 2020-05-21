import { authHeader, apiUrl } from '../_helpers';

export const serviceService = {
    getAll,
    getById
};

const requestOptions = {
    method: 'GET',
    headers: authHeader()
};

async function getAll() {
    const response = await fetch(`${apiUrl}/service`, requestOptions);
    return handleResponse(response);
}

async function getById(id) {
    const response = await fetch(`${apiUrl}/service/${id}`, requestOptions);
    return handleResponse(response);
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