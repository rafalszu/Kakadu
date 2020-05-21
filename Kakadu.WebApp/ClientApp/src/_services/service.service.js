import { authHeader, apiUrl } from '../_helpers';

export const serviceService = {
    getAll,
    getByCode
};

const requestOptions = {
    method: 'GET',
    headers: authHeader()
};

async function getAll() {
    const response = await fetch(`${apiUrl}/service`, requestOptions);
    return handleResponse(response);
}

async function getByCode(code) {
    const response = await fetch(`${apiUrl}/service/${code}`, requestOptions);
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